using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Entities
{
    public class Department : BaseEntity
    {
        // navigation properties
        public ICollection<JobTitle> JobTitles { get; set; } = new List<JobTitle>();

        //fields
        [Required, MaxLength(30)]
        public string? Name { get; set; }

        [MaxLength(150)]
        public string? Description { get; set; }
    }
}
