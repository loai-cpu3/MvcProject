using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcProject.Models.Domain
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public string? SenderUserId { get; set; }
        [Required]
        public NotificationType Type { get; set; }
        [Required]
        public string Content { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;



        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        [ForeignKey("SenderUserId")]
        public ApplicationUser SenderUser { get; set; }
    }
}
