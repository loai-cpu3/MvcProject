using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcProject.Models.Domain
{
<<<<<<< HEAD
    public class Notification
    {
        [Key]
        public int Id { get; set; }
=======
    public class Notification : BaseEntity
    {
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2
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
<<<<<<< HEAD
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
=======
>>>>>>> 8ede1be8af9ec82583cabefb08c524b5a2f670d2



        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        [ForeignKey("SenderUserId")]
        public ApplicationUser SenderUser { get; set; }
    }
}
