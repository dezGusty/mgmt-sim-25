using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Entities
{
    [PrimaryKey(nameof(SecondManagerEmployeeId), nameof(ReplacedManagerId), nameof(StartDate))]
    public class SecondManager
    {
        public int SecondManagerEmployeeId { get; set; }
        public User SecondManagerEmployee { get; set; }

        public int ReplacedManagerId { get; set; }
        public User ReplacedManager { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive => DateTime.Now >= StartDate && DateTime.Now <= EndDate;
    }
} 