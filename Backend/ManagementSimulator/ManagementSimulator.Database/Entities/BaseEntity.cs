using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Entities
{
    public class BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        // Audit properties for tracking who performed the action
        // These are nullable to support existing data migration
        public int? CreatedBy { get; set; }

        public int? ModifiedBy { get; set; }

        public int? DeletedBy { get; set; }
    }
}
