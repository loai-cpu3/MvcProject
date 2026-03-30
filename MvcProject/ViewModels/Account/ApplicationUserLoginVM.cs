using System.ComponentModel.DataAnnotations;

namespace MvcProject.ViewModels
{
    public class ApplicationUserLoginVM
    {
        public string Email { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
