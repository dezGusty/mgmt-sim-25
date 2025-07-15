using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Entities
{
    public class JobTitle : BaseEntity
    {
        //navigation properties 
        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        //fields
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
