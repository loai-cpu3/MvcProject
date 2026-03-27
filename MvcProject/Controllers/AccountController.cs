using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using MvcProject.Models.Domain;
using MvcProject.ViewModels;
using MvcProject.ViewModels.Account;

namespace MvcProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
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
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index","Home");
        }
    }
}