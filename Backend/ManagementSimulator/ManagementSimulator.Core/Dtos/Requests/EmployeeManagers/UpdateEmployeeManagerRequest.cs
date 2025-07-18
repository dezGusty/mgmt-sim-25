using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.EmployeeManagers
{
    public class UpdateEmployeeForManagerRequest
    {
        public int NewEmployeeId { get; set; }
    }

    public class UpdateManagerForEmployeeRequest
    {
        public int NewManagerId { get; set; }
    }
}
