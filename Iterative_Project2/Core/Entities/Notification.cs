using Iterative_Project2.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Core.Entities
{
    public class Notification
    {
        // Primary Key
        public Guid Id { get; set; }

        // Foreign Key: the user who receives it
        public Guid UserId { get; set; }

        public Notificationtype Type { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        // Optional link to what it is about (application id, company id, ...)
        public Guid? RelatedEntityId { get; set; }

        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        // Navigation Property
        public User User { get; set; } = null!;
    }
}
