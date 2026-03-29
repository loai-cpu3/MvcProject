using System.ComponentModel.DataAnnotations;

namespace MvcProject.ViewModels.Account
{
    public class forgetPassVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}