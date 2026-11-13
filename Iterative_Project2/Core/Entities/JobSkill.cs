using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Iterative_Project2.Core.Entities
{
    public class JobSkill
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // Foreign Key
        public Guid JobId { get; set; }

        // Navigation Property
        public Job Job { get; set; } = null!;
    }
}
