using System.ComponentModel.DataAnnotations;

namespace MvcProject.Models.Domain
{
    public class ApplicationUser
    {
        [Key]
        public string Id { get; set; } 
        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string FirstName { get; set; }
        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        [MaxLength(500)]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; } 
        public string? ProfilePictureUrl { get; set; }



        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();
        public ICollection<ProjectTask> AssignedTasks { get; set; } = new List<ProjectTask>();
        public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    }
}
