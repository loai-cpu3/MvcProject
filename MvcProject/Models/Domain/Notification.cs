using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcProject.Models.Domain
{
    public class Notification : BaseEntity
    {
        [Required]
        public string UserId { get; set; } = null!;
        public string? SenderUserId { get; set; }
        [Required]
        public NotificationType Type { get; set; }
        [Required]
        public string Content { get; set; } = null!;
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public bool IsRead { get; set; } = false;



        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
        [ForeignKey("SenderUserId")]
        public ApplicationUser SenderUser { get; set; } = null!;
    }
}
