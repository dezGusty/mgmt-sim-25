using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Infrastructure.Exceptions
{
    public class UniqueConstraintViolationException : Exception
    {
        public string EntityName { get; }
        public string ViolatedPropertyName { get; }
        public UniqueConstraintViolationException(string entityName, string violatedPropertyName) :
            base($"The {entityName} '{violatedPropertyName}' already exists")
        {
            EntityName = entityName;
            ViolatedPropertyName = violatedPropertyName;
        }
    }
}
