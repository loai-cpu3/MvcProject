using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MvcProject.Models.Domain;
using MvcProject.ViewModels;
using MvcProject.ViewModels.Account;
using System.Security.Claims;

namespace MvcProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly EmailService _emailService;

        public AccountController(
     UserManager<ApplicationUser> userManager,
     SignInManager<ApplicationUser> signInManager,
     IWebHostEnvironment webHostEnvironment,
     EmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(ApplicationUserRegisterVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            string? profilePictureUrl = null;
            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(model.ProfilePicture.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ProfilePicture", "Only .jpg, .jpeg, .png, .webp files are allowed.");
                    return View(model);
                }


                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                Directory.CreateDirectory(uploadsFolder);


                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(stream);
                }

                profilePictureUrl = $"/uploads/profiles/{uniqueFileName}";
            }

            var user = new ApplicationUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email,
                ProfilePictureUrl = profilePictureUrl
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(ApplicationUserLoginVM loginReq)
        {
            if (ModelState.IsValid)
            {

                ApplicationUser? user = await _userManager.FindByEmailAsync(loginReq.Email);

                bool validCredentials = false;
                if (user != null)
                {
                    validCredentials = await _userManager.CheckPasswordAsync(user, loginReq.Password);
                }

                if (!validCredentials)
                {
                    ModelState.AddModelError(string.Empty, "Invalid credentials.");
                    return View("Login", loginReq);
                }

         

                await _signInManager.SignInAsync(user!, loginReq.RememberMe);
                return RedirectToAction("Index", "Home");
            }

            return View("Login", loginReq);
        }
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new AccountSettingsVM
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                CurrentProfilePictureUrl = user.ProfilePictureUrl
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(AccountSettingsVM model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                model.Email = user.Email ?? string.Empty;
                model.CurrentProfilePictureUrl = user.ProfilePictureUrl;
                return View(model);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            // Handle Profile Picture
            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(model.ProfilePicture.FileName).ToLower();

                if (allowedExtensions.Contains(extension))
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePicture.CopyToAsync(stream);
                    }

                    // Delete old image if it exists
                    if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                    {
                        var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfilePictureUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            try { System.IO.File.Delete(oldFilePath); } catch { /* Ignore delete errors */ }
                        }
                    }

                    user.ProfilePictureUrl = $"/uploads/profiles/{uniqueFileName}";
                }
                else
                {
                    ModelState.AddModelError("ProfilePicture", "Invalid file format. Only JPG, PNG, and WebP are allowed.");
                    model.Email = user.Email ?? string.Empty;
                    model.CurrentProfilePictureUrl = user.ProfilePictureUrl;
                    return View(model);
                }
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                model.Email = user.Email ?? string.Empty;
                model.CurrentProfilePictureUrl = user.ProfilePictureUrl;
                return View(model);
            }

            // Handle Password Change
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is required to set a new password.");
                    model.Email = user.Email ?? string.Empty;
                    model.CurrentProfilePictureUrl = user.ProfilePictureUrl;
                    return View(model);
                }

                var passwordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    model.Email = user.Email ?? string.Empty;
                    model.CurrentProfilePictureUrl = user.ProfilePictureUrl;
                    return View(model);
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Settings));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index","Home");
        }



      //google
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account",
                                         new { returnUrl });

            var properties = _signInManager
                .ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return Challenge(properties, provider);
        }

       
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null)
        {
         
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToAction(nameof(Login));

            var result = await _signInManager
                .ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey,
                                           isPersistent: false);

            if (result.Succeeded)
                return RedirectToLocal(returnUrl);

           
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "";
            var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "";

            if (email == null)
            {
                TempData["Error"] = "Could not retrieve email from external provider.";
                return RedirectToAction(nameof(Login));
            }

     
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
             
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName.Length >= 2 ? firstName : "User",
                    LastName = lastName.Length >= 2 ? lastName : "Name",
                    EmailConfirmed = true 
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    TempData["Error"] = "Account creation failed.";
                    return RedirectToAction(nameof(Login));
                }
            }

      
            await _userManager.AddLoginAsync(user, info);
            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToLocal(returnUrl);
        }

       
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }


        //
        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(forgetPassVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return RedirectToAction("ForgotPasswordConfirmation");

            // Generate token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Build reset link
            var resetLink = Url.Action("ResetPassword", "Account",
                new { token, email = user.Email }, Request.Scheme);

            // Build email body
            var emailBody = $@"
    <h2>Reset Your Password</h2>
    <p>You requested a password reset. Click the button below:</p>
    <a href='{resetLink}' 
       style='background:#4F46E5; color:white; padding:12px 24px; 
              text-decoration:none; border-radius:6px;'>
        Reset Password
    </a>
    <p>This link expires in 24 hours.</p>
    <p>If you did not request this, ignore this email.</p>
";

            // Send real email
            await _emailService.SendAsync(model.Email, "Reset Your Password", emailBody);

            return RedirectToAction("ForgotPasswordConfirmation");
        }
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
                return RedirectToAction("Login");

            var model = new resetPassVM { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(resetPassVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return RedirectToAction("ResetPasswordConfirmation");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

            if (result.Succeeded)
                return RedirectToAction("ResetPasswordConfirmation");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}