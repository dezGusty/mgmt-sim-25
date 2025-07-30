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
                var managerRole = roles.First(r => r.Rolename == "Manager");
                var employeeRole = roles.First(r => r.Rolename == "Employee");

                // Admin User
                var adminUser = new User
                {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    JobTitleId = itAdminTitle.Id,
                    Title = itAdminTitle,
                    DateOfEmployment = DateTime.UtcNow
                };

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

                // Manager User
                var managerUser = new User
                {
                    FirstName = "Manager",
                    LastName = "User",
                    Email = "manager@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"),
                    JobTitleId = itAdminTitle.Id,
                    Title = itAdminTitle,
                    DateOfEmployment = DateTime.UtcNow
                };

                dbContext.Users.Add(managerUser);
                dbContext.SaveChanges();

                var managerRoleUser = new EmployeeRoleUser
                {
                    UsersId = managerUser.Id,
                    RolesId = managerRole.Id,
                    Role = managerRole,
                    User = managerUser,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.EmployeeRolesUsers.Add(managerRoleUser);

                // Employee User
                var employeeUser = new User
                {
                    FirstName = "Employee",
                    LastName = "User",
                    Email = "employee@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("employee123"),
                    JobTitleId = itAdminTitle.Id,
                    Title = itAdminTitle,
                    DateOfEmployment = DateTime.UtcNow
                };

                dbContext.Users.Add(employeeUser);
                dbContext.SaveChanges();

                var employeeRoleUser = new EmployeeRoleUser
                {
                    UsersId = employeeUser.Id,
                    RolesId = employeeRole.Id,
                    Role = employeeRole,
                    User = employeeUser,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.EmployeeRolesUsers.Add(employeeRoleUser);

                dbContext.SaveChanges();
            }
        }
    }
}
