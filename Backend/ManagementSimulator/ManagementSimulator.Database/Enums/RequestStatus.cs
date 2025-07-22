using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Enums
{
    public enum RequestStatus
    {
        InvalidRequestStatus = 0,   
        Pending = 2,
        Approved = 4,
        Rejected = 8,
        Expired = 16,
    }
}
