using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcProject.Models.Domain
{
    public class Attachment : BaseEntity
    {
        // FK to ProjectTask
        public int ProjectTaskId { get; set; }

        [Required, MaxLength(260)]
        public string OriginalFileName { get; set; } = null!;

        [Required, MaxLength(260)]
        public string StoredFileName { get; set; } = null!; // GUID + ext on disk

        [Required, MaxLength(100)]
        public string ContentType { get; set; } = null!;

        public long Size { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ProjectTaskId")]
        public ProjectTask ProjectTask { get; set; }
    }
}