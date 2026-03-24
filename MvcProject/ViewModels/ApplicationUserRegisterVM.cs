using System.ComponentModel.DataAnnotations;

namespace MvcProject.ViewModels
{
    public class ApplicationUserRegisterVM
    {
        [Required(ErrorMessage = "First name is required.")]
        [MinLength(2, ErrorMessage = "First name must be at least 2 characters.")]
        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [MinLength(2, ErrorMessage = "Last name must be at least 2 characters.")]
        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
        public string LastName { get; set; }
        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MaxLength(100)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [MaxLength(100)]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password you entered does not match our records. Please check your password and try again.")]
        public string ConfirmPassowrd { get; set; }
        public IFormFile? ProfilePicture { get; set; }
    }
}
