using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Enums;

namespace ManagementSimulator.Infrastructure.Seeding
{
    public static class PopulateSeed
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
            var departments = new List<Department>
            {
                new() { Name = "IT", Description = "Information Technology" },
                new() { Name = "HR", Description = "Human Resources" },
                new() { Name = "Finance", Description = "Financial Department" },
                new() { Name = "Marketing", Description = "Marketing and PR" },
                new() { Name = "Executive", Description = "Executive Leadership Team" },
                new() { Name = "Strategy", Description = "Strategic Planning and Business Development" },
                new() { Name = "Operations", Description = "Operations Management" },
                new() { Name = "Sales", Description = "Sales and Revenue Generation" },
                new() { Name = "Customer Service", Description = "Customer Support and Relations" },
                new() { Name = "Business Development", Description = "New Business Opportunities" },
                new() { Name = "Account Management", Description = "Key Account Management" },
                new() { Name = "Product Management", Description = "Product Strategy and Management" },
                new() { Name = "Engineering", Description = "Software Engineering and Development" },
                new() { Name = "Research & Development", Description = "Innovation and R&D" },
                new() { Name = "Quality Assurance", Description = "Quality Control and Testing" },
                new() { Name = "DevOps", Description = "Development Operations and Infrastructure" },
                new() { Name = "Data Science", Description = "Data Analytics and Machine Learning" },
                new() { Name = "Legal", Description = "Legal Affairs and Compliance" },
                new() { Name = "Procurement", Description = "Purchasing and Vendor Management" },
                new() { Name = "Facilities", Description = "Office Management and Facilities" },
                new() { Name = "Security", Description = "Corporate Security and Safety" },
                new() { Name = "Administration", Description = "General Administration" },
                new() { Name = "Accounting", Description = "Financial Accounting and Reporting" },
                new() { Name = "Treasury", Description = "Treasury and Cash Management" },
                new() { Name = "Internal Audit", Description = "Internal Audit and Risk Management" },
                new() { Name = "Tax", Description = "Tax Planning and Compliance" },
                new() { Name = "Financial Planning", Description = "Financial Planning and Analysis" },
                new() { Name = "Digital Marketing", Description = "Online Marketing and Social Media" },
                new() { Name = "Brand Management", Description = "Brand Strategy and Management" },
                new() { Name = "Public Relations", Description = "Public Relations and Communications" },
                new() { Name = "Content Marketing", Description = "Content Creation and Marketing" },
                new() { Name = "Market Research", Description = "Market Analysis and Research" },
                new() { Name = "Talent Acquisition", Description = "Recruitment and Hiring" },
                new() { Name = "Learning & Development", Description = "Training and Professional Development" },
                new() { Name = "Compensation & Benefits", Description = "Salary and Benefits Administration" },
                new() { Name = "Employee Relations", Description = "Employee Relations and Engagement" },
                new() { Name = "Manufacturing", Description = "Production and Manufacturing" },
                new() { Name = "Supply Chain", Description = "Supply Chain Management" },
                new() { Name = "Logistics", Description = "Logistics and Distribution" },
                new() { Name = "Warehouse", Description = "Warehouse Operations" },
                new() { Name = "International", Description = "International Business Operations" },
                new() { Name = "Regional Operations", Description = "Regional Business Management" },
                new() { Name = "Corporate Communications", Description = "Internal and External Communications" },
                new() { Name = "Investor Relations", Description = "Shareholder and Investor Relations" },
                new() { Name = "Sustainability", Description = "Environmental and Social Responsibility" },
                new() { Name = "Innovation Lab", Description = "Innovation and Future Technologies" },
                new() { Name = "Partnership", Description = "Strategic Partnerships and Alliances" },
                new() { Name = "Regulatory Affairs", Description = "Regulatory Compliance and Affairs" }
            };

            foreach (var dept in departments)
            {
                if (!dbContext.Departments.Any(d => d.Name == dept.Name))
                {
                    dbContext.Departments.Add(dept);
                }
            }
            dbContext.SaveChanges();

            var leaveTypes = new List<LeaveRequestType>
            {
                new() { Title = "Vacation", Description = "Standard annual leave", IsPaid = true, MaxDays = 21 },
                new() { Title = "Sick Leave", Description = "Medical certificate required", IsPaid = true, MaxDays = 183  },
                new() { Title = "Parental Leave", Description = "Applicable for new parents", IsPaid = true, MaxDays = 730 },
                new() { Title = "Study Leave", Description = "Educational purposes and training", IsPaid = false, MaxDays = 10  },
                new() { Title = "Military Leave", Description = "Military service obligations", IsPaid = false, MaxDays = 1000 },
                new() { Title = "Study Leave", Description = "Educational purposes and training", IsPaid = true, MaxDays = 10 },
            };

            foreach (var leaveType in leaveTypes)
            {
                if (!dbContext.LeaveRequestTypes.Any(l => l.Description == leaveType.Description))
                {
                    dbContext.LeaveRequestTypes.Add(leaveType);
                }
            }
            dbContext.SaveChanges();

            var departmentMap = dbContext.Departments.ToDictionary(d => d.Name, d => d.Id);

            var jobTitles = new List<JobTitle>
            {
                new() { Name = "Software Engineer" },
                new() { Name = "System Administrator" },
                new() { Name = "IT Manager" },
                new() { Name = "Network Administrator" },
                new() { Name = "Database Administrator" },
                new() { Name = "IT Support Specialist" },
                new() { Name = "Cybersecurity Analyst" },
                new() { Name = "Cloud Architect" },
                new() { Name = "HR Specialist" },
                new() { Name = "Recruiter" },
                new() { Name = "HR Manager" },
                new() { Name = "HR Business Partner" },
                new() { Name = "HR Generalist" },
                new() { Name = "HR Director" },
                new() { Name = "Accountant" },
                new() { Name = "Financial Analyst" },
                new() { Name = "Finance Manager" },
                new() { Name = "Controller" },
                new() { Name = "CFO" },
                new() { Name = "Budget Analyst" },
                new() { Name = "Marketing Manager" },
                new() { Name = "Content Creator" },
                new() { Name = "Marketing Specialist" },
                new() { Name = "Marketing Director" },
                new() { Name = "Brand Manager" },
                new() { Name = "CEO" },
                new() { Name = "COO" },
                new() { Name = "Vice President" },
                new() { Name = "Executive Assistant" },
                new() { Name = "Strategy Analyst" },
                new() { Name = "Business Analyst" },
                new() { Name = "Strategic Planning Manager" },
                new() { Name = "Management Consultant" },
                new() { Name = "Operations Manager" },
                new() { Name = "Operations Analyst" },
                new() { Name = "Process Improvement Specialist" },
                new() { Name = "Operations Director" },
                new() { Name = "Sales Representative" },
                new() { Name = "Sales Manager" },
                new() { Name = "Senior Sales Executive" },
                new() { Name = "Sales Director" },
                new() { Name = "Inside Sales Representative" },
                new() { Name = "Customer Service Representative" },
                new() { Name = "Customer Success Manager" },
                new() { Name = "Customer Service Manager" },
                new() { Name = "Technical Support Specialist" },
                new() { Name = "Business Development Manager" },
                new() { Name = "Business Development Representative" },
                new() { Name = "Partnership Manager" },
                new() { Name = "Account Manager" },
                new() { Name = "Key Account Manager" },
                new() { Name = "Client Relationship Manager" },
                new() { Name = "Product Manager" },
                new() { Name = "Senior Product Manager" },
                new() { Name = "Product Owner" },
                new() { Name = "Product Director" },
                new() { Name = "Frontend Developer" },
                new() { Name = "Backend Developer" },
                new() { Name = "Full Stack Developer" },
                new() { Name = "Engineering Manager" },
                new() { Name = "Lead Engineer" },
                new() { Name = "Principal Engineer" },
                new() { Name = "Research Scientist" },
                new() { Name = "R&D Engineer" },
                new() { Name = "Innovation Manager" },
                new() { Name = "R&D Director" },
                new() { Name = "QA Engineer" },
                new() { Name = "QA Manager" },
                new() { Name = "Test Automation Engineer" },
                new() { Name = "Quality Control Specialist" },
                new() { Name = "DevOps Engineer" },
                new() { Name = "Site Reliability Engineer" },
                new() { Name = "Infrastructure Engineer" },
                new() { Name = "DevOps Manager" },
                new() { Name = "Data Scientist" },
                new() { Name = "Data Analyst" },
                new() { Name = "Machine Learning Engineer" },
                new() { Name = "Data Engineer" },
                new() { Name = "Senior Data Scientist" },
                new() { Name = "Corporate Lawyer" },
                new() { Name = "Legal Counsel" },
                new() { Name = "Compliance Officer" },
                new() { Name = "Legal Manager" },
                new() { Name = "Procurement Specialist" },
                new() { Name = "Purchasing Manager" },
                new() { Name = "Vendor Manager" },
                new() { Name = "Contract Manager" },
                new() { Name = "Facilities Manager" },
                new() { Name = "Maintenance Technician" },
                new() { Name = "Office Manager" },
                new() { Name = "Building Coordinator" },
                new() { Name = "Security Officer" },
                new() { Name = "Security Manager" },
                new() { Name = "Safety Coordinator" },
                new() { Name = "Risk Management Specialist" },
                new() { Name = "Administrative Assistant" },
                new() { Name = "Office Coordinator" },
                new() { Name = "Executive Secretary" },
                new() { Name = "Administrative Manager" }
            };

            foreach (var title in jobTitles)
            {
                if (!dbContext.JobTitles.Any(jt => jt.Name == title.Name))
                {
                    dbContext.JobTitles.Add(title);
                }
            }
            dbContext.SaveChanges();

            var jobTitlesList = dbContext.JobTitles.ToList();
            if (!jobTitlesList.Any()) return;

            var roles = dbContext.EmployeeRoles.ToList();
            var adminRoleId = roles.FirstOrDefault(r => r.Rolename == "Admin")?.Id ?? 0;
            var managerRoleId = roles.FirstOrDefault(r => r.Rolename == "Manager")?.Id ?? 0;
            var employeeRoleId = roles.FirstOrDefault(r => r.Rolename == "Employee")?.Id ?? 0;

            var departmentsList = dbContext.Departments.ToList();

            var userSeeds = new List<(User User, List<int> RoleIds)>
            {
                (new User {
                    FirstName = "John",
                    LastName = "Anderson",
                    Email = "john.anderson@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "CEO").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Executive").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2005, 3, 1),
                    EmploymentType = EmploymentType.FullTime
                }, new List<int> { adminRoleId, managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Sarah",
                    LastName = "Mitchell",
                    Email = "sarah.mitchell@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "COO").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Executive").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2008, 6, 15),
                    EmploymentType = EmploymentType.PartTime
                }, new List<int> { adminRoleId, managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Michael",
                    LastName = "Thompson",
                    Email = "michael.thompson@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "CFO").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Finance").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2007, 2, 10),
                    EmploymentType = EmploymentType.FullTime
                }, new List<int> { adminRoleId, managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Emily",
                    LastName = "Davis",
                    Email = "emily.davis@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Vice President").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Executive").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2020, 8, 22)
                }, new List<int> { adminRoleId, managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Robert",
                    LastName = "Wilson",
                    Email = "robert.wilson@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Vice President").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Executive").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2021, 4, 7)
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "David",
                    LastName = "Garcia",
                    Email = "david.garcia@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "IT Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "IT").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2012, 11, 3)
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Jennifer",
                    LastName = "Martinez",
                    Email = "jennifer.martinez@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Software Engineer").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Engineering").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2022, 1, 18)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "James",
                    LastName = "Rodriguez",
                    Email = "james.rodriguez@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Software Engineer").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Engineering").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2021, 9, 12)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Lisa",
                    LastName = "Hernandez",
                    Email = "lisa.hernandez@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Software Engineer").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Engineering").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2023, 3, 25)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Christopher",
                    LastName = "Lopez",
                    Email = "christopher.lopez@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Network Administrator").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "IT").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2021, 7, 14)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Brandon",
                    LastName = "Allen",
                    Email = "brandon.allen@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "System Administrator").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "IT").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2022, 5, 8)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Connor",
                    LastName = "Mitchell",
                    Email = "connor.mitchell@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Engineering Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Engineering").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2020, 12, 20)
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Alexis",
                    LastName = "Carter",
                    Email = "alexis.carter@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Lead Engineer").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Engineering").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2021, 10, 5)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Blake",
                    LastName = "Roberts",
                    Email = "blake.roberts@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Principal Engineer").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Engineering").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2009, 4, 30)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Morgan",
                    LastName = "Gomez",
                    Email = "morgan.gomez@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Frontend Developer").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Engineering").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2022, 8, 16)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Cameron",
                    LastName = "Phillips",
                    Email = "cameron.phillips@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Backend Developer").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Engineering").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2023, 1, 9)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Gregory",
                    LastName = "James",
                    Email = "gregory.james@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Sales Director").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Sales").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2006, 11, 12)
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Victoria",
                    LastName = "Watson",
                    Email = "victoria.watson@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Sales Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Sales").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2021, 6, 28)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Adam",
                    LastName = "Brooks",
                    Email = "adam.brooks@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Sales Representative").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Sales").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2022, 11, 14)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Emma",
                    LastName = "Price",
                    Email = "emma.price@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Sales Representative").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Sales").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2023, 7, 3)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Charlotte",
                    LastName = "Perry",
                    Email = "charlotte.perry@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Marketing Director").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Marketing").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2011, 2, 17)
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Henry",
                    LastName = "Powell",
                    Email = "henry.powell@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Marketing Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Marketing").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2021, 12, 6)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Amelia",
                    LastName = "Long",
                    Email = "amelia.long@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Brand Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Marketing").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2022, 4, 21)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Ethan",
                    LastName = "Flores",
                    Email = "ethan.flores@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Content Creator").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Marketing").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2023, 2, 13)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Lucas",
                    LastName = "Butler",
                    Email = "lucas.butler@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "HR Director").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "HR").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2004, 8, 26)
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Abigail",
                    LastName = "Simmons",
                    Email = "abigail.simmons@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "HR Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "HR").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2021, 3, 11)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Gabriel",
                    LastName = "Foster",
                    Email = "gabriel.foster@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "HR Business Partner").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "HR").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2022, 9, 7)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Scarlett",
                    LastName = "Alexander",
                    Email = "scarlett.alexander@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Recruiter").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "HR").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2023, 5, 19)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Owen",
                    LastName = "Russell",
                    Email = "owen.russell@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Finance Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Finance").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2013, 10, 24)
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Grace",
                    LastName = "Griffin",
                    Email = "grace.griffin@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Controller").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Finance").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2021, 1, 15)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Sebastian",
                    LastName = "Diaz",
                    Email = "sebastian.diaz@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Financial Analyst").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Finance").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2022, 7, 29)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Jack",
                    LastName = "Myers",
                    Email = "jack.myers@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Accountant").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Finance").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2023, 4, 12)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Zoey",
                    LastName = "Graham",
                    Email = "zoey.graham@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Customer Service Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Customer Service").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2014, 5, 18)
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Luke",
                    LastName = "Sullivan",
                    Email = "luke.sullivan@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Customer Success Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Customer Service").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2021, 11, 23)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Layla",
                    LastName = "Wallace",
                    Email = "layla.wallace@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Customer Service Representative").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Customer Service").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2022, 12, 10)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Wyatt",
                    LastName = "West",
                    Email = "wyatt.west@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Operations Director").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Operations").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2003, 9, 4)
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Nora",
                    LastName = "Tucker",
                    Email = "nora.tucker@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Operations Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Operations").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2021, 2, 27)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Leo",
                    LastName = "Parker",
                    Email = "leo.parker@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Operations Analyst").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Operations").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2022, 6, 1)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Alex",
                    LastName = "Rogers",
                    Email = "alex.rogers@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Senior Data Scientist").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Data Science").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2015, 7, 13)
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Vanessa",
                    LastName = "Reed",
                    Email = "vanessa.reed@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Data Scientist").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Data Science").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2021, 8, 25)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Carlos",
                    LastName = "Bailey",
                    Email = "carlos.bailey@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Data Analyst").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Data Science").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2023, 6, 8)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Isaiah",
                    LastName = "Thompson",
                    Email = "isaiah.thompson@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "QA Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Quality Assurance").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2016, 3, 16)
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Aurora",
                    LastName = "White",
                    Email = "aurora.white@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "QA Engineer").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Quality Assurance").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2022, 10, 31)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Grayson",
                    LastName = "Martinez",
                    Email = "grayson.martinez@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "DevOps Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "DevOps").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2021, 5, 2)
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Penelope",
                    LastName = "Anderson",
                    Email = "penelope.anderson@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "DevOps Engineer").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "DevOps").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2023, 8, 14)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Adrian",
                    LastName = "Moore",
                    Email = "adrian.moore@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Product Director").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Product Management").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2002, 12, 9)
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Addison",
                    LastName = "Martin",
                    Email = "addison.martin@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Senior Product Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Product Management").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2022, 2, 22)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Lincoln",
                    LastName = "Garcia",
                    Email = "lincoln.garcia@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Legal Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Legal").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2022, 2, 22)
                }, new List<int> { managerRoleId, employeeRoleId }),
                (new User {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "System Administrator").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "IT").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2010, 1, 15)
                }, new List<int> { adminRoleId, managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "John",
                    LastName = "Anderson",
                    Email = "john.anderson@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "CEO").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Executive").Id
                }, new List<int> { adminRoleId, managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Sarah",
                    LastName = "Mitchell",
                    Email = "sarah.mitchell@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "COO").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Executive").Id
                }, new List<int> { adminRoleId, managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Michael",
                    LastName = "Thompson",
                    Email = "michael.thompson@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "CFO").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Finance").Id
                }, new List<int> { adminRoleId, managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Emily",
                    LastName = "Davis",
                    Email = "emily.davis@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Vice President").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Executive").Id
                }, new List<int> { adminRoleId, managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Robert",
                    LastName = "Wilson",
                    Email = "robert.wilson@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Vice President").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Executive").Id
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "David",
                    LastName = "Garcia",
                    Email = "david.garcia@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "IT Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "IT").Id
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Jennifer",
                    LastName = "Martinez",
                    Email = "jennifer.martinez@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Software Engineer").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Engineering").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "James",
                    LastName = "Rodriguez",
                    Email = "james.rodriguez@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Software Engineer").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Engineering").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Lisa",
                    LastName = "Hernandez",
                    Email = "lisa.hernandez@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Software Engineer").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Engineering").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Christopher",
                    LastName = "Lopez",
                    Email = "christopher.lopez@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Network Administrator").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "IT").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Brandon",
                    LastName = "Allen",
                    Email = "brandon.allen@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "System Administrator").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "IT").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Connor",
                    LastName = "Mitchell",
                    Email = "connor.mitchell@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Engineering Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Engineering").Id
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Alexis",
                    LastName = "Carter",
                    Email = "alexis.carter@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Lead Engineer").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Engineering").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Blake",
                    LastName = "Roberts",
                    Email = "blake.roberts@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Principal Engineer").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Engineering").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Morgan",
                    LastName = "Gomez",
                    Email = "morgan.gomez@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Frontend Developer").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Engineering").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Cameron",
                    LastName = "Phillips",
                    Email = "cameron.phillips@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Backend Developer").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Engineering").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Gregory",
                    LastName = "James",
                    Email = "gregory.james@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Sales Director").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Sales").Id
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Victoria",
                    LastName = "Watson",
                    Email = "victoria.watson@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Sales Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Sales").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Adam",
                    LastName = "Brooks",
                    Email = "adam.brooks@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Sales Representative").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Sales").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Emma",
                    LastName = "Price",
                    Email = "emma.price@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Sales Representative").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Sales").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Charlotte",
                    LastName = "Perry",
                    Email = "charlotte.perry@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Marketing Director").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Marketing").Id
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Henry",
                    LastName = "Powell",
                    Email = "henry.powell@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Marketing Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Marketing").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Amelia",
                    LastName = "Long",
                    Email = "amelia.long@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Brand Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Marketing").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Ethan",
                    LastName = "Flores",
                    Email = "ethan.flores@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Content Creator").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Marketing").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Lucas",
                    LastName = "Butler",
                    Email = "lucas.butler@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "HR Director").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "HR").Id
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Abigail",
                    LastName = "Simmons",
                    Email = "abigail.simmons@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "HR Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "HR").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Gabriel",
                    LastName = "Foster",
                    Email = "gabriel.foster@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "HR Business Partner").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "HR").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Scarlett",
                    LastName = "Alexander",
                    Email = "scarlett.alexander@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Recruiter").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "HR").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Owen",
                    LastName = "Russell",
                    Email = "owen.russell@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Finance Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Finance").Id
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Grace",
                    LastName = "Griffin",
                    Email = "grace.griffin@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Controller").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Finance").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Sebastian",
                    LastName = "Diaz",
                    Email = "sebastian.diaz@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Financial Analyst").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Finance").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Jack",
                    LastName = "Myers",
                    Email = "jack.myers@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Accountant").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Finance").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Zoey",
                    LastName = "Graham",
                    Email = "zoey.graham@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Customer Service Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Customer Service").Id
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Luke",
                    LastName = "Sullivan",
                    Email = "luke.sullivan@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Customer Success Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Customer Service").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Layla",
                    LastName = "Wallace",
                    Email = "layla.wallace@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Customer Service Representative").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Customer Service").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Wyatt",
                    LastName = "West",
                    Email = "wyatt.west@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Operations Director").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Operations").Id
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Nora",
                    LastName = "Tucker",
                    Email = "nora.tucker@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Operations Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Operations").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Leo",
                    LastName = "Parker",
                    Email = "leo.parker@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Operations Analyst").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Operations").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Alex",
                    LastName = "Rogers",
                    Email = "alex.rogers@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Senior Data Scientist").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Data Science").Id
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Vanessa",
                    LastName = "Reed",
                    Email = "vanessa.reed@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Data Scientist").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Data Science").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Carlos",
                    LastName = "Bailey",
                    Email = "carlos.bailey@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Data Analyst").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Data Science").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Isaiah",
                    LastName = "Thompson",
                    Email = "isaiah.thompson@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "QA Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Quality Assurance").Id
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Aurora",
                    LastName = "White",
                    Email = "aurora.white@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "QA Engineer").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Quality Assurance").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Grayson",
                    LastName = "Martinez",
                    Email = "grayson.martinez@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "DevOps Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "DevOps").Id
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Penelope",
                    LastName = "Anderson",
                    Email = "penelope.anderson@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "DevOps Engineer").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "DevOps").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Adrian",
                    LastName = "Moore",
                    Email = "adrian.moore@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Product Director").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Product Management").Id
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Addison",
                    LastName = "Martin",
                    Email = "addison.martin@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Senior Product Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Product Management").Id
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Lincoln",
                    LastName = "Garcia",
                    Email = "lincoln.garcia@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Legal Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Legal").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2017, 4, 6)
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Elena",
                    LastName = "Rodriguez",
                    Email = "elena.rodriguez@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Corporate Lawyer").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Legal").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2021, 4, 19)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Mateo",
                    LastName = "Flores",
                    Email = "mateo.flores@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "R&D Director").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Research & Development").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2001, 10, 11)
                }, new List<int> { managerRoleId, employeeRoleId }),

                (new User {
                    FirstName = "Genesis",
                    LastName = "Green",
                    Email = "genesis.green@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Research Scientist").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Research & Development").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2022, 3, 28)
                }, new List<int> { employeeRoleId }),

                (new User {
                    FirstName = "Serenity",
                    LastName = "Nelson",
                    Email = "serenity.nelson@simulator.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Innovation Manager").Id,
                    DepartmentId = departmentsList.First(d => d.Name == "Research & Development").Id,
                    MustChangePassword = false,
                    DateOfEmployment = new DateTime(2023, 9, 15)
                }, new List<int> { adminRoleId, employeeRoleId })
            };
            foreach (var (user, roleIds) in userSeeds)
            {
                if (!dbContext.Users.Any(u => u.Email == user.Email))
                {
                    // Set availability based on employment type
                    SetAvailabilityForEmploymentType(user);

                    dbContext.Users.Add(user);
                    dbContext.SaveChanges();

                    foreach (var roleId in roleIds)
                    {
                        if (roleId == 0) continue;

                        dbContext.EmployeeRolesUsers.Add(new EmployeeRoleUser
                        {
                            UsersId = user.Id,
                            RolesId = roleId,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            dbContext.SaveChanges();

            // Ensure EmploymentType and Availability are set if missing or zero
            var usersNeedingAvailabilityUpdate = dbContext.Users
                .Where(u => u.TotalAvailability == 0f || u.RemainingAvailability == 0f)
                .ToList();

            foreach (var u in usersNeedingAvailabilityUpdate)
            {
                if (u.EmploymentType != EmploymentType.FullTime && u.EmploymentType != EmploymentType.PartTime)
                {
                    u.EmploymentType = EmploymentType.FullTime;
                }

                SetAvailabilityForEmploymentType(u);
            }

            if (usersNeedingAvailabilityUpdate.Count > 0)
            {
                dbContext.SaveChanges();
            }

            var rnd = new Random();
            for (int i = 1; i <= 500; i++)
            {
                var email = $"test{i}@simulator.com";
                if (dbContext.Users.Any(u => u.Email == email))
                    continue;

                var job = jobTitlesList[rnd.Next(jobTitlesList.Count)];
                var department = departmentsList[rnd.Next(departmentsList.Count)];

                var user = new User
                {
                    FirstName = $"TestF{i}",
                    LastName = $"TestL{i}",
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass123"),
                    JobTitleId = job.Id,
                    DepartmentId = department.Id,
                    EmploymentType = EmploymentType.FullTime
                };

                SetAvailabilityForEmploymentType(user);

                dbContext.Users.Add(user);
                dbContext.SaveChanges();

                dbContext.EmployeeRolesUsers.Add(new EmployeeRoleUser
                {
                    UsersId = user.Id,
                    RolesId = employeeRoleId,
                    CreatedAt = DateTime.UtcNow
                });

                if (rnd.Next(1, 101) <= 20 && managerRoleId != 0)
                {
                    dbContext.EmployeeRolesUsers.Add(new EmployeeRoleUser
                    {
                        UsersId = user.Id,
                        RolesId = managerRoleId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            dbContext.SaveChanges();

            var allUsers = dbContext.Users.Where(u => !u.Email.StartsWith("test")).ToList();
            var managerUsers = allUsers.Where(u => dbContext.EmployeeRolesUsers
                .Any(eru => eru.UsersId == u.Id && eru.RolesId == managerRoleId && eru.DeletedAt == null))
                .ToList();

            var employeeManagerRelationships = new List<(int EmployeeId, int ManagerId)>();

            var itManager = allUsers.FirstOrDefault(u => u.Email == "david.garcia@simulator.com");
            if (itManager != null)
            {
                var itEmployees = allUsers.Where(u => new[] {
                    "jennifer.martinez@simulator.com", "james.rodriguez@simulator.com",
                    "lisa.hernandez@simulator.com", "christopher.lopez@simulator.com",
                    "brandon.allen@simulator.com"
                }.Contains(u.Email)).ToList();

                foreach (var emp in itEmployees)
                {
                    employeeManagerRelationships.Add((emp.Id, itManager.Id));
                }
            }

            var engManager = allUsers.FirstOrDefault(u => u.Email == "connor.mitchell@simulator.com");
            if (engManager != null)
            {
                var engEmployees = allUsers.Where(u => new[] {
                    "alexis.carter@simulator.com", "blake.roberts@simulator.com",
                    "morgan.gomez@simulator.com", "cameron.phillips@simulator.com"
                }.Contains(u.Email)).ToList();

                foreach (var emp in engEmployees)
                {
                    employeeManagerRelationships.Add((emp.Id, engManager.Id));
                }
            }

            var salesManager = allUsers.FirstOrDefault(u => u.Email == "gregory.james@simulator.com");
            if (salesManager != null)
            {
                var salesEmployees = allUsers.Where(u => new[] {
                    "victoria.watson@simulator.com", "adam.brooks@simulator.com",
                    "emma.price@simulator.com"
                }.Contains(u.Email)).ToList();

                foreach (var emp in salesEmployees)
                {
                    employeeManagerRelationships.Add((emp.Id, salesManager.Id));
                }
            }

            var marketingManager = allUsers.FirstOrDefault(u => u.Email == "charlotte.perry@simulator.com");
            if (marketingManager != null)
            {
                var marketingEmployees = allUsers.Where(u => new[] {
                    "henry.powell@simulator.com", "amelia.long@simulator.com",
                    "ethan.flores@simulator.com"
                }.Contains(u.Email)).ToList();

                foreach (var emp in marketingEmployees)
                {
                    employeeManagerRelationships.Add((emp.Id, marketingManager.Id));
                }
            }

            var hrManager = allUsers.FirstOrDefault(u => u.Email == "lucas.butler@simulator.com");
            if (hrManager != null)
            {
                var hrEmployees = allUsers.Where(u => new[] {
                    "abigail.simmons@simulator.com", "gabriel.foster@simulator.com",
                    "scarlett.alexander@simulator.com"
                }.Contains(u.Email)).ToList();

                foreach (var emp in hrEmployees)
                {
                    employeeManagerRelationships.Add((emp.Id, hrManager.Id));
                }
            }

            var financeManager = allUsers.FirstOrDefault(u => u.Email == "owen.russell@simulator.com");
            if (financeManager != null)
            {
                var financeEmployees = allUsers.Where(u => new[] {
                    "grace.griffin@simulator.com", "sebastian.diaz@simulator.com",
                    "jack.myers@simulator.com"
                }.Contains(u.Email)).ToList();

                foreach (var emp in financeEmployees)
                {
                    employeeManagerRelationships.Add((emp.Id, financeManager.Id));
                }
            }

            var csManager = allUsers.FirstOrDefault(u => u.Email == "zoey.graham@simulator.com");
            if (csManager != null)
            {
                var csEmployees = allUsers.Where(u => new[] {
                    "luke.sullivan@simulator.com", "layla.wallace@simulator.com"
                }.Contains(u.Email)).ToList();

                foreach (var emp in csEmployees)
                {
                    employeeManagerRelationships.Add((emp.Id, csManager.Id));
                }
            }

            var opsManager = allUsers.FirstOrDefault(u => u.Email == "wyatt.west@simulator.com");
            if (opsManager != null)
            {
                var opsEmployees = allUsers.Where(u => new[] {
                    "nora.tucker@simulator.com", "leo.parker@simulator.com"
                }.Contains(u.Email)).ToList();

                foreach (var emp in opsEmployees)
                {
                    employeeManagerRelationships.Add((emp.Id, opsManager.Id));
                }
            }

            var dataManager = allUsers.FirstOrDefault(u => u.Email == "alex.rogers@simulator.com");
            if (dataManager != null)
            {
                var dataEmployees = allUsers.Where(u => new[] {
                    "vanessa.reed@simulator.com", "carlos.bailey@simulator.com"
                }.Contains(u.Email)).ToList();

                foreach (var emp in dataEmployees)
                {
                    employeeManagerRelationships.Add((emp.Id, dataManager.Id));
                }
            }

            var qaManager = allUsers.FirstOrDefault(u => u.Email == "isaiah.thompson@simulator.com");
            if (qaManager != null)
            {
                var qaEmployees = allUsers.Where(u => new[] {
                    "aurora.white@simulator.com"
                }.Contains(u.Email)).ToList();

                foreach (var emp in qaEmployees)
                {
                    employeeManagerRelationships.Add((emp.Id, qaManager.Id));
                }
            }

            var devopsManager = allUsers.FirstOrDefault(u => u.Email == "grayson.martinez@simulator.com");
            if (devopsManager != null)
            {
                var devopsEmployees = allUsers.Where(u => new[] {
                    "penelope.anderson@simulator.com"
                }.Contains(u.Email)).ToList();

                foreach (var emp in devopsEmployees)
                {
                    employeeManagerRelationships.Add((emp.Id, devopsManager.Id));
                }
            }

            var productManager = allUsers.FirstOrDefault(u => u.Email == "adrian.moore@simulator.com");
            if (productManager != null)
            {
                var productEmployees = allUsers.Where(u => new[] {
                    "addison.martin@simulator.com"
                }.Contains(u.Email)).ToList();

                foreach (var emp in productEmployees)
                {
                    employeeManagerRelationships.Add((emp.Id, productManager.Id));
                }
            }

            var legalManager = allUsers.FirstOrDefault(u => u.Email == "lincoln.garcia@simulator.com");
            if (legalManager != null)
            {
                var legalEmployees = allUsers.Where(u => new[] {
                    "elena.rodriguez@simulator.com"
                }.Contains(u.Email)).ToList();

                foreach (var emp in legalEmployees)
                {
                    employeeManagerRelationships.Add((emp.Id, legalManager.Id));
                }
            }

            var rdManager = allUsers.FirstOrDefault(u => u.Email == "mateo.flores@simulator.com");
            if (rdManager != null)
            {
                var rdEmployees = allUsers.Where(u => new[] {
                    "genesis.green@simulator.com"
                }.Contains(u.Email)).ToList();

                foreach (var emp in rdEmployees)
                {
                    employeeManagerRelationships.Add((emp.Id, rdManager.Id));
                }
            }

            var ceo = allUsers.FirstOrDefault(u => u.Email == "john.anderson@simulator.com");
            if (ceo != null)
            {
                var departmentManagers = allUsers.Where(u => new[] {
                    "david.garcia@simulator.com", "connor.mitchell@simulator.com",
                    "gregory.james@simulator.com", "charlotte.perry@simulator.com",
                    "lucas.butler@simulator.com", "owen.russell@simulator.com",
                    "zoey.graham@simulator.com", "wyatt.west@simulator.com",
                    "alex.rogers@simulator.com", "isaiah.thompson@simulator.com",
                    "grayson.martinez@simulator.com", "adrian.moore@simulator.com",
                    "lincoln.garcia@simulator.com", "mateo.flores@simulator.com"
                }.Contains(u.Email)).ToList();

                foreach (var manager in departmentManagers)
                {
                    employeeManagerRelationships.Add((manager.Id, ceo.Id));
                }
            }

            foreach (var (employeeId, managerId) in employeeManagerRelationships)
            {
                if (!dbContext.EmployeeManagers.Any(em => em.EmployeeId == employeeId && em.ManagerId == managerId && em.DeletedAt == null))
                {
                    dbContext.EmployeeManagers.Add(new EmployeeManager
                    {
                        EmployeeId = employeeId,
                        ManagerId = managerId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            var testUsers = dbContext.Users.Where(u => u.Email.StartsWith("test")).ToList();
            var allManagers = dbContext.Users.Where(u => dbContext.EmployeeRolesUsers
                .Any(eru => eru.UsersId == u.Id && eru.RolesId == managerRoleId && eru.DeletedAt == null))
                .ToList();

            var existingRelationships = dbContext.EmployeeManagers
                .Where(em => em.DeletedAt == null)
                .Select(em => new { em.EmployeeId, em.ManagerId })
                .ToHashSet();

            var newRelationships = new List<EmployeeManager>();

            var random = new Random();
            foreach (var testUser in testUsers)
            {
                if (random.Next(1, 101) <= 70 && allManagers.Any())
                {
                    var randomManager = allManagers[random.Next(allManagers.Count)];

                    if (!existingRelationships.Contains(new { EmployeeId = testUser.Id, ManagerId = randomManager.Id }) &&
                        !newRelationships.Any(nr => nr.EmployeeId == testUser.Id && nr.ManagerId == randomManager.Id))
                    {
                        newRelationships.Add(new EmployeeManager
                        {
                            EmployeeId = testUser.Id,
                            ManagerId = randomManager.Id,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            var testManagers = testUsers.Where(u => dbContext.EmployeeRolesUsers
                .Any(eru => eru.UsersId == u.Id && eru.RolesId == managerRoleId && eru.DeletedAt == null))
                .ToList();

            var testEmployeesWithoutManagers = testUsers.Where(u =>
                !existingRelationships.Any(er => er.EmployeeId == u.Id) &&
                !newRelationships.Any(nr => nr.EmployeeId == u.Id))
                .ToList();

            foreach (var testManager in testManagers)
            {
                var teamSize = random.Next(3, 9);
                var availableEmployees = testEmployeesWithoutManagers.Take(teamSize).ToList();

                foreach (var employee in availableEmployees)
                {
                    if (!existingRelationships.Contains(new { EmployeeId = employee.Id, ManagerId = testManager.Id }) &&
                        !newRelationships.Any(nr => nr.EmployeeId == employee.Id && nr.ManagerId == testManager.Id))
                    {
                        newRelationships.Add(new EmployeeManager
                        {
                            EmployeeId = employee.Id,
                            ManagerId = testManager.Id,
                            CreatedAt = DateTime.UtcNow
                        });

                        testEmployeesWithoutManagers.Remove(employee);
                    }
                }
            }

            foreach (var employee in testEmployeesWithoutManagers)
            {
                if (allManagers.Any())
                {
                    var randomManager = allManagers[random.Next(allManagers.Count)];

                    dbContext.EmployeeManagers.Add(new EmployeeManager
                    {
                        EmployeeId = employee.Id,
                        ManagerId = randomManager.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            dbContext.SaveChanges();

            var allLeaveTypes = dbContext.LeaveRequestTypes.ToList();
            var allUsersForLeave = dbContext.Users.ToList();
            var allManagersForReview = dbContext.Users.Where(u =>
                dbContext.EmployeeRolesUsers.Any(eru => eru.UsersId == u.Id && eru.RolesId == managerRoleId && eru.DeletedAt == null)
            ).ToList();

            var leaveRequests = new List<LeaveRequest>();
            var leaveStatuses = Enum.GetValues(typeof(ManagementSimulator.Database.Enums.RequestStatus));
            var random2 = new Random();

            var userLeavePeriods = new Dictionary<int, List<(DateTime start, DateTime end)>>();

            var userLeaveDays = new Dictionary<int, int>();

            int maxRequests = 10000;
            int attempts = 0;
            while (leaveRequests.Count < maxRequests && attempts < maxRequests * 10)
            {
                attempts++;

                var user = allUsersForLeave[random2.Next(allUsersForLeave.Count)];
                var leaveType = allLeaveTypes[random2.Next(allLeaveTypes.Count)];
                var reviewer = allManagersForReview.Count > 0 ? allManagersForReview[random2.Next(allManagersForReview.Count)] : null;

                var startDate = DateTime.UtcNow.AddDays(-random2.Next(0, 365));
                var maxLength = Math.Min(15, 30 - (userLeaveDays.ContainsKey(user.Id) ? userLeaveDays[user.Id] : 0));
                if (maxLength < 1) continue;

                var length = random2.Next(1, maxLength + 1);
                var endDate = startDate.AddDays(length);

                if (endDate <= startDate) continue;

                if (!userLeavePeriods.ContainsKey(user.Id))
                    userLeavePeriods[user.Id] = new List<(DateTime, DateTime)>();

                bool overlaps = userLeavePeriods[user.Id].Any(p =>
                    (startDate < p.end && endDate > p.start)
                );
                if (overlaps) continue;

                leaveRequests.Add(new LeaveRequest
                {
                    UserId = user.Id,
                    ReviewerId = reviewer?.Id,
                    LeaveRequestTypeId = leaveType.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    Reason = $"Auto-generated reason {leaveRequests.Count}",
                    RequestStatus = (ManagementSimulator.Database.Enums.RequestStatus)(leaveStatuses.GetValue(random2.Next(leaveStatuses.Length)) ?? ManagementSimulator.Database.Enums.RequestStatus.Pending),
                    ReviewerComment = reviewer != null ? $"Reviewed by {reviewer.FirstName}" : null,
                    CreatedAt = startDate.AddDays(-random2.Next(0, 10))
                });

                userLeavePeriods[user.Id].Add((startDate, endDate));
                if (!userLeaveDays.ContainsKey(user.Id))
                    userLeaveDays[user.Id] = 0;
                userLeaveDays[user.Id] += (int)(endDate - startDate).TotalDays;
            }

            dbContext.LeaveRequests.AddRange(leaveRequests);
            dbContext.SaveChanges();

            // Seed Projects
            var projects = new List<Project>
            {
                new() {
                    Name = "Digital Transformation Initiative",
                    StartDate = DateTime.UtcNow.AddMonths(-18),
                    EndDate = DateTime.UtcNow.AddMonths(6),
                    BudgetedFTEs = 12.5f,
                    IsActive = true
                },
                new() {
                    Name = "Cloud Migration Project",
                    StartDate = DateTime.UtcNow.AddMonths(-12),
                    EndDate = DateTime.UtcNow.AddMonths(3),
                    BudgetedFTEs = 8.0f,
                    IsActive = true
                },
                new() {
                    Name = "Employee Portal Redesign",
                    StartDate = DateTime.UtcNow.AddMonths(-6),
                    EndDate = DateTime.UtcNow.AddMonths(2),
                    BudgetedFTEs = 5.5f,
                    IsActive = true
                },
                new() {
                    Name = "Cybersecurity Enhancement Program",
                    StartDate = DateTime.UtcNow.AddMonths(-24),
                    EndDate = DateTime.UtcNow.AddMonths(-6),
                    BudgetedFTEs = 6.0f,
                    IsActive = false
                },
                new() {
                    Name = "Customer Data Analytics Platform",
                    StartDate = DateTime.UtcNow.AddMonths(-15),
                    EndDate = DateTime.UtcNow.AddMonths(9),
                    BudgetedFTEs = 10.0f,
                    IsActive = true
                },
                new() {
                    Name = "Supply Chain Optimization",
                    StartDate = DateTime.UtcNow.AddMonths(-8),
                    EndDate = DateTime.UtcNow.AddMonths(4),
                    BudgetedFTEs = 7.5f,
                    IsActive = true
                },
                new() {
                    Name = "Mobile App Development",
                    StartDate = DateTime.UtcNow.AddMonths(-10),
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    BudgetedFTEs = 9.0f,
                    IsActive = true
                },
                new() {
                    Name = "Enterprise Resource Planning Upgrade",
                    StartDate = DateTime.UtcNow.AddMonths(-36),
                    EndDate = DateTime.UtcNow.AddMonths(-18),
                    BudgetedFTEs = 15.0f,
                    IsActive = false
                },
                new() {
                    Name = "Artificial Intelligence Research Initiative",
                    StartDate = DateTime.UtcNow.AddMonths(-3),
                    EndDate = DateTime.UtcNow.AddMonths(15),
                    BudgetedFTEs = 4.5f,
                    IsActive = true
                },
                new() {
                    Name = "Remote Work Infrastructure",
                    StartDate = DateTime.UtcNow.AddMonths(-30),
                    EndDate = DateTime.UtcNow.AddMonths(-12),
                    BudgetedFTEs = 11.0f,
                    IsActive = false
                },
                new() {
                    Name = "Compliance Management System",
                    StartDate = DateTime.UtcNow.AddMonths(-4),
                    EndDate = DateTime.UtcNow.AddMonths(8),
                    BudgetedFTEs = 3.5f,
                    IsActive = true
                },
                new() {
                    Name = "Business Intelligence Dashboard",
                    StartDate = DateTime.UtcNow.AddMonths(-14),
                    EndDate = DateTime.UtcNow.AddMonths(-2),
                    BudgetedFTEs = 6.5f,
                    IsActive = false
                },
                new() {
                    Name = "Marketing Automation Platform",
                    StartDate = DateTime.UtcNow.AddMonths(-7),
                    EndDate = DateTime.UtcNow.AddMonths(5),
                    BudgetedFTEs = 4.0f,
                    IsActive = true
                },
                new() {
                    Name = "Financial Reporting Modernization",
                    StartDate = DateTime.UtcNow.AddMonths(-20),
                    EndDate = DateTime.UtcNow.AddMonths(-8),
                    BudgetedFTEs = 8.5f,
                    IsActive = false
                },
                new() {
                    Name = "Customer Support Chatbot",
                    StartDate = DateTime.UtcNow.AddMonths(-2),
                    EndDate = DateTime.UtcNow.AddMonths(6),
                    BudgetedFTEs = 2.5f,
                    IsActive = true
                },
                new() {
                    Name = "IoT Sensor Network Implementation",
                    StartDate = DateTime.UtcNow.AddMonths(1),
                    EndDate = DateTime.UtcNow.AddMonths(18),
                    BudgetedFTEs = 7.0f,
                    IsActive = true
                },
                new() {
                    Name = "Blockchain Integration Pilot",
                    StartDate = DateTime.UtcNow.AddMonths(2),
                    EndDate = DateTime.UtcNow.AddMonths(14),
                    BudgetedFTEs = 3.0f,
                    IsActive = true
                },
                new() {
                    Name = "Legacy System Decommissioning",
                    StartDate = DateTime.UtcNow.AddMonths(-5),
                    EndDate = DateTime.UtcNow.AddMonths(3),
                    BudgetedFTEs = 5.0f,
                    IsActive = true
                }
            };

            foreach (var project in projects)
            {
                if (!dbContext.Projects.Any(p => p.Name == project.Name))
                {
                    dbContext.Projects.Add(project);
                }
            }
            dbContext.SaveChanges();

            // Get all projects and users for assignments
            var allProjects = dbContext.Projects.ToList();
            var allUsersForProjects = dbContext.Users.Where(u => !u.Email.StartsWith("test")).ToList();
            var adminUser = allUsersForProjects.FirstOrDefault(u => u.Email == "admin@simulator.com");

            // Create user-project assignments
            var userProjectAssignments = new List<UserProject>();
            var randomAssignment = new Random();

            foreach (var project in allProjects.Where(p => p.IsActive))
            {
                var teamSize = randomAssignment.Next(3, Math.Min(8, allUsersForProjects.Count));
                var projectTeam = allUsersForProjects.OrderBy(x => randomAssignment.Next()).Take(teamSize).ToList();

                foreach (var user in projectTeam)
                {
                    var timePercentage = randomAssignment.Next(10, 60) / 100.0f; // 10% to 60%

                    if (!dbContext.UserProjects.Any(up => up.UserId == user.Id && up.ProjectId == project.Id))
                    {
                        userProjectAssignments.Add(new UserProject
                        {
                            UserId = user.Id,
                            ProjectId = project.Id,
                            TimePercentagePerProject = timePercentage
                        });
                    }
                }
            }

            dbContext.UserProjects.AddRange(userProjectAssignments);
            dbContext.SaveChanges();

            // Generate extensive audit logs for admin activities
            if (adminUser != null)
            {
                var auditLogs = new List<AuditLog>();
                var randomAudit = new Random();
                var startDate = DateTime.UtcNow.AddMonths(-12);
                var endDate = DateTime.UtcNow;

                var actions = new[] { "CREATE", "UPDATE", "DELETE", "ASSIGN", "UNASSIGN", "APPROVE", "REJECT", "LOGIN", "LOGOUT", "EXPORT", "IMPORT" };
                var entityTypes = new[] { "Project", "User", "Department", "LeaveRequest", "UserProject", "Role", "Report", "Settings" };
                var httpMethods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH" };
                var endpoints = new[] {
                    "/api/projects", "/api/users", "/api/departments", "/api/leaverequests",
                    "/api/userprojects", "/api/roles", "/api/reports", "/api/settings",
                    "/api/admin/dashboard", "/api/admin/users", "/api/admin/projects"
                };

                for (int i = 0; i < 800; i++)
                {
                    var timestamp = startDate.AddDays(randomAudit.Next(0, (int)(endDate - startDate).TotalDays))
                                             .AddHours(randomAudit.Next(8, 20)) // Business hours mostly
                                             .AddMinutes(randomAudit.Next(0, 60));

                    var action = actions[randomAudit.Next(actions.Length)];
                    var entityType = entityTypes[randomAudit.Next(entityTypes.Length)];
                    var httpMethod = httpMethods[randomAudit.Next(httpMethods.Length)];
                    var endpoint = endpoints[randomAudit.Next(endpoints.Length)];

                    var entityId = randomAudit.Next(1, 100);
                    var entityName = entityType switch
                    {
                        "Project" => allProjects.Count > 0 ? allProjects[randomAudit.Next(allProjects.Count)].Name : $"Project {entityId}",
                        "User" => allUsersForProjects.Count > 0 ? $"{allUsersForProjects[randomAudit.Next(allUsersForProjects.Count)].FirstName} {allUsersForProjects[randomAudit.Next(allUsersForProjects.Count)].LastName}" : $"User {entityId}",
                        "Department" => departmentsList.Count > 0 ? departmentsList[randomAudit.Next(departmentsList.Count)].Name : $"Department {entityId}",
                        _ => $"{entityType} {entityId}"
                    };

                    var description = action switch
                    {
                        "CREATE" => $"Created new {entityType.ToLower()}: {entityName}",
                        "UPDATE" => $"Updated {entityType.ToLower()}: {entityName}",
                        "DELETE" => $"Deleted {entityType.ToLower()}: {entityName}",
                        "ASSIGN" => $"Assigned user to {entityType.ToLower()}: {entityName}",
                        "UNASSIGN" => $"Unassigned user from {entityType.ToLower()}: {entityName}",
                        "APPROVE" => $"Approved {entityType.ToLower()}: {entityName}",
                        "REJECT" => $"Rejected {entityType.ToLower()}: {entityName}",
                        "LOGIN" => "Administrator logged into system",
                        "LOGOUT" => "Administrator logged out of system",
                        "EXPORT" => $"Exported {entityType.ToLower()} data",
                        "IMPORT" => $"Imported {entityType.ToLower()} data",
                        _ => $"Performed {action.ToLower()} on {entityType.ToLower()}: {entityName}"
                    };

                    var oldValues = action == "UPDATE" ? $"{{\"name\":\"{entityName}_old\",\"status\":\"active\"}}" : null;
                    var newValues = action == "CREATE" || action == "UPDATE" ? $"{{\"name\":\"{entityName}\",\"status\":\"active\",\"modifiedBy\":\"admin\"}}" : null;

                    auditLogs.Add(new AuditLog
                    {
                        Action = action,
                        EntityType = entityType,
                        EntityId = entityId,
                        EntityName = entityName,
                        UserId = adminUser.Id,
                        UserEmail = adminUser.Email,
                        UserRoles = "Admin,Manager,Employee",
                        IsImpersonating = false,
                        HttpMethod = httpMethod,
                        Endpoint = endpoint,
                        IpAddress = $"192.168.1.{randomAudit.Next(100, 255)}",
                        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36",
                        OldValues = oldValues,
                        NewValues = newValues,
                        AdditionalData = $"{{\"sessionId\":\"{Guid.NewGuid()}\",\"requestId\":\"{randomAudit.Next(10000, 99999)}\"}}",
                        Timestamp = timestamp,
                        Success = randomAudit.Next(1, 101) <= 95, // 95% success rate
                        ErrorMessage = randomAudit.Next(1, 101) <= 5 ? "Operation failed due to validation error" : null,
                        Description = description
                    });
                }

                // Add some weekend and after-hours activities for thorough admin coverage
                for (int i = 0; i < 100; i++)
                {
                    var timestamp = startDate.AddDays(randomAudit.Next(0, (int)(endDate - startDate).TotalDays));

                    // Weekend work
                    if (timestamp.DayOfWeek == DayOfWeek.Saturday || timestamp.DayOfWeek == DayOfWeek.Sunday)
                    {
                        timestamp = timestamp.AddHours(randomAudit.Next(10, 18));
                    }
                    else
                    {
                        // After hours work
                        timestamp = timestamp.AddHours(randomAudit.Next(20, 24));
                    }

                    auditLogs.Add(new AuditLog
                    {
                        Action = "LOGIN",
                        EntityType = "System",
                        EntityId = null,
                        EntityName = "Management Simulator",
                        UserId = adminUser.Id,
                        UserEmail = adminUser.Email,
                        UserRoles = "Admin,Manager,Employee",
                        IsImpersonating = false,
                        HttpMethod = "POST",
                        Endpoint = "/api/auth/login",
                        IpAddress = $"192.168.1.{randomAudit.Next(100, 255)}",
                        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                        Timestamp = timestamp,
                        Success = true,
                        Description = "Administrator after-hours system access"
                    });
                }

                dbContext.AuditLogs.AddRange(auditLogs);
                dbContext.SaveChanges();
            }
        }
    }
}