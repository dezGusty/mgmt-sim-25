using ManagementSimulator.Database.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ManagementSimulator.Database.Entities
{
    public class User : BaseEntity
    {
        // navigation properties
        public int JobTitleId { get; set; }
        public JobTitle Title { get; set; }


        // fields
        [Required,MaxLength(50),EmailAddress]
        public string Email { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        public UserRole Role { get; set; } = UserRole.InvalidRole;

        public string PasswordHash { get; set; }
    }
}
