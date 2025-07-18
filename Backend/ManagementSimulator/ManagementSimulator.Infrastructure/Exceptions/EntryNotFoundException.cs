using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Infrastructure.Exceptions
{
    public class EntryNotFoundException : Exception
    {
        public string EntityName { get; }
        public object EntityId { get; }

        public EntryNotFoundException(string entityName, object entityId) 
             : base($"{entityName} with id '{entityId}' was not found.")
        {
            EntityName = entityName;
            EntityId = entityId;
        }
    }
}
