using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Enums;

namespace ManagementSimulator.Infrastructure
{
    public static class SetupSeed
    {
        private static void SetAvailabilityForEmploymentType(User user)
        {
            switch (user.EmploymentType)
            {
                case EmploymentType.FullTime:
                    user.TotalAvailability = 1.0f;
                    user.RemainingAvailability = 1.0f;
                    break;
                case EmploymentType.PartTime:
                    user.TotalAvailability = 0.5f;
                    user.RemainingAvailability = 0.5f;
                    break;
                default:
                    user.TotalAvailability = 1.0f;
                    user.RemainingAvailability = 1.0f;
                    break;
            }
        }

        public static void Seed(MGMTSimulatorDbContext dbContext)
        {
            var itDepartment = dbContext.Departments.FirstOrDefault(d => d.Name == "IT");
            if (itDepartment == null)
            {
                itDepartment = new Department { Name = "IT" };
                dbContext.Departments.Add(itDepartment);
                dbContext.SaveChanges();
            }

            var adminTitle = dbContext.JobTitles.FirstOrDefault(jt => jt.Name == "Administrator");
            if (adminTitle == null)
            {
                adminTitle = new JobTitle
                {
                    Name = "Administrator",
                };
                dbContext.JobTitles.Add(adminTitle);
                dbContext.SaveChanges();
            }

            var roleNames = new[] { "Admin", "Manager", "Employee", "HR" };
            var roles = new List<EmployeeRole>();
            foreach (var roleName in roleNames)
            {
                var role = dbContext.EmployeeRoles.FirstOrDefault(r => r.Rolename == roleName);
                if (role == null)
                {
                    role = new EmployeeRole { Rolename = roleName };
                    dbContext.EmployeeRoles.Add(role);
                    dbContext.SaveChanges();
                }
                roles.Add(role);
            }

            if (!dbContext.Users.Any(u => u.Email == "admin@ftd.com"))
            {
                var adminRole = roles.First(r => r.Rolename == "Admin");
                var managerRole = roles.First(r => r.Rolename == "Manager");
                var employeeRole = roles.First(r => r.Rolename == "Employee");
                var hrRole = roles.First(r => r.Rolename == "HR");

                var adminUser = new User
                {
                    FirstName = "System",
                    LastName = "Administrator",
                    Email = "admin@ftd.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = adminTitle.Id,
                    DepartmentId = itDepartment.Id,
                    Title = adminTitle,
                    DateOfEmployment = DateTime.UtcNow,
                    EmploymentType = EmploymentType.FullTime
                };

                SetAvailabilityForEmploymentType(adminUser);

                dbContext.Users.Add(adminUser);
                dbContext.SaveChanges();

                var adminRoleUser = new EmployeeRoleUser
                {
                    UsersId = adminUser.Id,
                    RolesId = adminRole.Id,
                    Role = adminRole,
                    User = adminUser,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.EmployeeRolesUsers.Add(adminRoleUser);

                var adminManagerRoleUser = new EmployeeRoleUser
                {
                    UsersId = adminUser.Id,
                    RolesId = managerRole.Id,
                    Role = managerRole,
                    User = adminUser,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.EmployeeRolesUsers.Add(adminManagerRoleUser);

                var adminEmployeeRoleUser = new EmployeeRoleUser
                {
                    UsersId = adminUser.Id,
                    RolesId = employeeRole.Id,
                    Role = employeeRole,
                    User = adminUser,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.EmployeeRolesUsers.Add(adminEmployeeRoleUser);

                var adminHrRoleUser = new EmployeeRoleUser
                {
                    UsersId = adminUser.Id,
                    RolesId = hrRole.Id,
                    Role = hrRole,
                    User = adminUser,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.EmployeeRolesUsers.Add(adminHrRoleUser);

                dbContext.SaveChanges();
            }
        }
    }
}
