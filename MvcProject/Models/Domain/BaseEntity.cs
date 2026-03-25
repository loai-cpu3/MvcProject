using System;
using System.ComponentModel.DataAnnotations;

namespace MvcProject.Models.Domain
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
