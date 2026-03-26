using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MvcProject.Models.Domain;
using MvcProject.ViewModels;

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index","Home");
        }
    }
}