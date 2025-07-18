using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.UserManagers
{
    public class CreateEmployeeManagerRequest
    {
        public int EmployeeId { get; set; }
        public int ManagerId { get; set; }
    }
}
