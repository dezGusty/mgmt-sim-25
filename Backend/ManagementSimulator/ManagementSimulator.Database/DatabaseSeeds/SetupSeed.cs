using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;

namespace ManagementSimulator.Infrastructure
{
    public static class SetupSeed
    {
        public static void Seed(MGMTSimulatorDbContext dbContext)
        {
            var itDepartment = dbContext.Departments.FirstOrDefault(d => d.Name == "IT");
            if (itDepartment == null)
            {
                itDepartment = new Department { Name = "IT" };
                dbContext.Departments.Add(itDepartment);
                dbContext.SaveChanges();
            }

            var itAdminTitle = dbContext.JobTitles.FirstOrDefault(jt => jt.Name == "ITAdmin" && jt.DepartmentId == itDepartment.Id);
            if (itAdminTitle == null)
            {
                itAdminTitle = new JobTitle
                {
                    Name = "ITAdmin",
                    DepartmentId = itDepartment.Id,
                    Department = itDepartment
                };
                dbContext.JobTitles.Add(itAdminTitle);
                dbContext.SaveChanges();
            }

            var roleNames = new[] { "Admin", "Manager", "Employee" };
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

            if (!dbContext.Users.Any(u => u.Email == "admin@simulator.com"))
            {
                var adminRole = roles.First(r => r.Rolename == "Admin");

                var adminUser = new User
                {
                    FirstName = "admin",
                    LastName = "admin",
                    Email = "admin@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    JobTitleId = itAdminTitle.Id,
                    Title = itAdminTitle
                };

                dbContext.Users.Add(adminUser);
                dbContext.SaveChanges();

                var roleUser = new EmployeeRoleUser
                {
                    UsersId = adminUser.Id,
                    RolesId = adminRole.Id,
                    Role = adminRole,
                    User = adminUser,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.EmployeeRolesUsers.Add(roleUser);
                dbContext.SaveChanges();
            }
        }
    }
}
