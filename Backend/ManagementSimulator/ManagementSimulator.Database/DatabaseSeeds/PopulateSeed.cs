using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;

namespace ManagementSimulator.Infrastructure.Seeding
{
    public static class PopulateSeed
    {
        public static void Seed(MGMTSimulatorDbContext dbContext)
        {
            // Seed Departments
            var departments = new List<Department>
            {
                new() { Name = "IT", Description = "Information Technology" },
                new() { Name = "HR", Description = "Human Resources" },
                new() { Name = "Finance", Description = "Financial Department" },
                new() { Name = "Marketing", Description = "Marketing and PR" }
            };

            foreach (var dept in departments)
            {
                if (!dbContext.Departments.Any(d => d.Name == dept.Name))
                {
                    dbContext.Departments.Add(dept);
                }
            }
            dbContext.SaveChanges();

            // Seed LeaveRequestTypes
            var leaveTypes = new List<LeaveRequestType>
            {
                new() { Description = "Vacation", AdditionalDetails = "Standard annual leave" },
                new() { Description = "Sick Leave", AdditionalDetails = "Medical certificate required" },
                new() { Description = "Parental Leave", AdditionalDetails = "Applicable for new parents" },
                new() { Description = "Unpaid Leave", AdditionalDetails = "Requires manager approval" }
            };

            foreach (var leaveType in leaveTypes)
            {
                if (!dbContext.LeaveRequestTypes.Any(l => l.Description == leaveType.Description))
                {
                    dbContext.LeaveRequestTypes.Add(leaveType);
                }
            }
            dbContext.SaveChanges();

            // Seed JobTitles
            var departmentMap = dbContext.Departments.ToDictionary(d => d.Name, d => d.Id);

            var jobTitles = new List<JobTitle>
            {
                new() { Name = "Software Engineer", DepartmentId = departmentMap["IT"] },
                new() { Name = "System Administrator", DepartmentId = departmentMap["IT"] },
                new() { Name = "HR Specialist", DepartmentId = departmentMap["HR"] },
                new() { Name = "Recruiter", DepartmentId = departmentMap["HR"] },
                new() { Name = "Accountant", DepartmentId = departmentMap["Finance"] },
                new() { Name = "Financial Analyst", DepartmentId = departmentMap["Finance"] },
                new() { Name = "Marketing Manager", DepartmentId = departmentMap["Marketing"] },
                new() { Name = "Content Creator", DepartmentId = departmentMap["Marketing"] }
            };

            foreach (var title in jobTitles)
            {
                if (!dbContext.JobTitles.Any(jt => jt.Name == title.Name && jt.DepartmentId == title.DepartmentId))
                {
                    dbContext.JobTitles.Add(title);
                }
            }
            dbContext.SaveChanges();

            // Seed Users
            var jobTitlesList = dbContext.JobTitles.ToList();
            if (!jobTitlesList.Any()) return;

            var users = new List<User>
            {
                new User { FirstName = "Admin", LastName = "User", Email = "admin@simulator.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), JobTitleId = jobTitlesList.First(jt => jt.Name == "System Administrator").Id },
                new User { FirstName = "Manager", LastName = "One", Email = "manager1@simulator.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"), JobTitleId = jobTitlesList.First(jt => jt.Name == "Software Engineer").Id },
                new User { FirstName = "Manager", LastName = "Two", Email = "manager2@simulator.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"), JobTitleId = jobTitlesList.First(jt => jt.Name == "Software Engineer").Id },
                new User { FirstName = "Employee", LastName = "One", Email = "employee1@simulator.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("employee123"), JobTitleId = jobTitlesList.First(jt => jt.Name == "Accountant").Id },
                new User { FirstName = "Employee", LastName = "Two", Email = "employee2@simulator.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("employee123"), JobTitleId = jobTitlesList.First(jt => jt.Name == "Recruiter").Id },
                new User { FirstName = "Employee", LastName = "Three", Email = "employee3@simulator.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("employee123"), JobTitleId = jobTitlesList.First(jt => jt.Name == "HR Specialist").Id },
                new User { FirstName = "Employee", LastName = "Four", Email = "employee4@simulator.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("employee123"), JobTitleId = jobTitlesList.First(jt => jt.Name == "Content Creator").Id },
                new User { FirstName = "Employee", LastName = "Five", Email = "employee5@simulator.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("employee123"), JobTitleId = jobTitlesList.First(jt => jt.Name == "Financial Analyst").Id },
                new User { FirstName = "Employee", LastName = "Six", Email = "employee6@simulator.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("employee123"), JobTitleId = jobTitlesList.First(jt => jt.Name == "Marketing Manager").Id },
                new User { FirstName = "Employee", LastName = "Seven", Email = "employee7@simulator.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("employee123"), JobTitleId = jobTitlesList.First(jt => jt.Name == "Software Engineer").Id }
            };

            foreach (var user in users)
            {
                if (!dbContext.Users.Any(u => u.Email == user.Email))
                {
                    dbContext.Users.Add(user);
                }
            }
            dbContext.SaveChanges();

            // Seed EmployeeRolesUsers
            var roles = dbContext.EmployeeRoles.ToList();
            var usersList = dbContext.Users.ToList();

            var roleAssignments = new List<(string Email, string RoleName)>
            {
                ("admin@simulator.com", "Admin"),
                ("manager1@simulator.com", "Manager"),
                ("manager2@simulator.com", "Manager"),
                ("employee1@simulator.com", "User"),
                ("employee2@simulator.com", "User"),
                ("employee3@simulator.com", "User"),
                ("employee3@simulator.com", "Manager"),
                ("employee4@simulator.com", "Admin"),
                ("employee4@simulator.com", "Manager"),
                ("employee5@simulator.com", "User"),
                ("employee5@simulator.com", "Admin"),
                ("employee6@simulator.com", "Admin"),
                ("employee6@simulator.com", "Manager"),
                ("employee6@simulator.com", "User"),
                ("employee7@simulator.com", "User")
            };

            foreach (var (email, roleName) in roleAssignments)
            {
                var user = usersList.FirstOrDefault(u => u.Email == email);
                if (user == null) continue;

                var role = roles.FirstOrDefault(r => r.Rolename == roleName);
                if (role == null) continue;

                var existingRelation = dbContext.EmployeeRolesUsers
                    .FirstOrDefault(e => e.UsersId == user.Id && e.RolesId == role.Id);

                if (existingRelation != null)
                {
                    if (existingRelation.DeletedAt != null)
                    {
                        existingRelation.DeletedAt = null;
                        existingRelation.ModifiedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    dbContext.EmployeeRolesUsers.Add(new EmployeeRoleUser
                    {
                        UsersId = user.Id,
                        RolesId = role.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            dbContext.SaveChanges();
        }
    }
}