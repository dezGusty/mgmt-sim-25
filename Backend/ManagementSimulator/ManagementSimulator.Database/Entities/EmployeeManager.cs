using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Entities
{
    [PrimaryKey(nameof(EmployeeId), nameof(ManagerId))]
    public class EmployeeManager 
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ModifiedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int EmployeeId { get; set; }
        public User Employee { get; set; } = null!;

        public int ManagerId { get; set; }
        public User Manager { get; set; } = null!;
    }
}
