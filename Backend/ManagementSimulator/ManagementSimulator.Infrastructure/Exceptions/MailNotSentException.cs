using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Infrastructure.Exceptions
{
    public class MailNotSentException : Exception
    {
        public string MailToSend { get; }

        public MailNotSentException(string mailToSend)
             : base($"Couldn't send mail to {mailToSend}")
        {
            MailToSend = mailToSend;
        }
    }
}
