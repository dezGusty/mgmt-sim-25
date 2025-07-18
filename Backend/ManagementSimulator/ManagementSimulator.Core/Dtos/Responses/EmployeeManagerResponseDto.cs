using ManagementSimulator.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Responses
{
    public class EmployeeManagerResponseDto
    {
        public int EmployeeId { get; set; }    
        public int ManagerId { get; set; }  
    }
}
