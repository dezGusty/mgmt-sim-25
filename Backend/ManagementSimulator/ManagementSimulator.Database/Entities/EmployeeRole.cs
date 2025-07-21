using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Entities
{
    public class EmployeeRole : BaseEntity
    {
        // navigation properties 
        public ICollection<User> Users { get; set; } = new List<User>();

        // fields
        [MaxLength(50)]
        public string Rolename { get; set; }
    }
}
