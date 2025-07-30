using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Dtos.Requests.ResetPassword
{
    public class SendResetCodeRequestDto
    {
        public string Email { get; set; } = string.Empty;
    }
}
