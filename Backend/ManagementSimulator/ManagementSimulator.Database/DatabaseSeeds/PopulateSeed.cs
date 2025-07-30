using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;

namespace ManagementSimulator.Infrastructure.Seeding
{
    public static class PopulateSeed
    {
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
                new() { Description = "Vacation", AdditionalDetails = "Standard annual leave" },
                new() { Description = "Sick Leave", AdditionalDetails = "Medical certificate required" },
                new() { Description = "Parental Leave", AdditionalDetails = "Applicable for new parents" },
                new() { Description = "Unpaid Leave", AdditionalDetails = "Requires manager approval" },
                new() { Description = "Personal Leave", AdditionalDetails = "Personal time off for individual matters" },
                new() { Description = "Bereavement Leave", AdditionalDetails = "Leave for death of family member" },
                new() { Description = "Emergency Leave", AdditionalDetails = "Urgent personal or family emergencies" },
                new() { Description = "Medical Leave", AdditionalDetails = "Extended medical treatment or recovery" },
                new() { Description = "Study Leave", AdditionalDetails = "Educational purposes and training" },
                new() { Description = "Jury Duty", AdditionalDetails = "Legal obligation for court service" },
                new() { Description = "Military Leave", AdditionalDetails = "Military service obligations" },
                new() { Description = "Sabbatical", AdditionalDetails = "Extended leave for personal development" },
                new() { Description = "Compensatory Time", AdditionalDetails = "Time off in lieu of overtime pay" },
                new() { Description = "Religious Leave", AdditionalDetails = "Religious observances and holidays" },
                new() { Description = "Volunteer Leave", AdditionalDetails = "Approved volunteer activities" },
                new() { Description = "Mental Health Leave", AdditionalDetails = "Mental health and wellness support" }
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
                new() { Name = "Software Engineer", DepartmentId = departmentMap["IT"] },
                new() { Name = "System Administrator", DepartmentId = departmentMap["IT"] },
                new() { Name = "IT Manager", DepartmentId = departmentMap["IT"] },
                new() { Name = "Network Administrator", DepartmentId = departmentMap["IT"] },
                new() { Name = "Database Administrator", DepartmentId = departmentMap["IT"] },
                new() { Name = "IT Support Specialist", DepartmentId = departmentMap["IT"] },
                new() { Name = "Cybersecurity Analyst", DepartmentId = departmentMap["IT"] },
                new() { Name = "Cloud Architect", DepartmentId = departmentMap["IT"] },
                new() { Name = "HR Specialist", DepartmentId = departmentMap["HR"] },
                new() { Name = "Recruiter", DepartmentId = departmentMap["HR"] },
                new() { Name = "HR Manager", DepartmentId = departmentMap["HR"] },
                new() { Name = "HR Business Partner", DepartmentId = departmentMap["HR"] },
                new() { Name = "HR Generalist", DepartmentId = departmentMap["HR"] },
                new() { Name = "HR Director", DepartmentId = departmentMap["HR"] },
                new() { Name = "Accountant", DepartmentId = departmentMap["Finance"] },
                new() { Name = "Financial Analyst", DepartmentId = departmentMap["Finance"] },
                new() { Name = "Finance Manager", DepartmentId = departmentMap["Finance"] },
                new() { Name = "Controller", DepartmentId = departmentMap["Finance"] },
                new() { Name = "CFO", DepartmentId = departmentMap["Finance"] },
                new() { Name = "Budget Analyst", DepartmentId = departmentMap["Finance"] },
                new() { Name = "Marketing Manager", DepartmentId = departmentMap["Marketing"] },
                new() { Name = "Content Creator", DepartmentId = departmentMap["Marketing"] },
                new() { Name = "Marketing Specialist", DepartmentId = departmentMap["Marketing"] },
                new() { Name = "Marketing Director", DepartmentId = departmentMap["Marketing"] },
                new() { Name = "Brand Manager", DepartmentId = departmentMap["Marketing"] },
                new() { Name = "CEO", DepartmentId = departmentMap["Executive"] },
                new() { Name = "COO", DepartmentId = departmentMap["Executive"] },
                new() { Name = "Vice President", DepartmentId = departmentMap["Executive"] },
                new() { Name = "Executive Assistant", DepartmentId = departmentMap["Executive"] },
                new() { Name = "Strategy Analyst", DepartmentId = departmentMap["Strategy"] },
                new() { Name = "Business Analyst", DepartmentId = departmentMap["Strategy"] },
                new() { Name = "Strategic Planning Manager", DepartmentId = departmentMap["Strategy"] },
                new() { Name = "Management Consultant", DepartmentId = departmentMap["Strategy"] },
                new() { Name = "Operations Manager", DepartmentId = departmentMap["Operations"] },
                new() { Name = "Operations Analyst", DepartmentId = departmentMap["Operations"] },
                new() { Name = "Process Improvement Specialist", DepartmentId = departmentMap["Operations"] },
                new() { Name = "Operations Director", DepartmentId = departmentMap["Operations"] },
                new() { Name = "Sales Representative", DepartmentId = departmentMap["Sales"] },
                new() { Name = "Sales Manager", DepartmentId = departmentMap["Sales"] },
                new() { Name = "Senior Sales Executive", DepartmentId = departmentMap["Sales"] },
                new() { Name = "Sales Director", DepartmentId = departmentMap["Sales"] },
                new() { Name = "Inside Sales Representative", DepartmentId = departmentMap["Sales"] },
                new() { Name = "Customer Service Representative", DepartmentId = departmentMap["Customer Service"] },
                new() { Name = "Customer Success Manager", DepartmentId = departmentMap["Customer Service"] },
                new() { Name = "Customer Service Manager", DepartmentId = departmentMap["Customer Service"] },
                new() { Name = "Technical Support Specialist", DepartmentId = departmentMap["Customer Service"] },
                new() { Name = "Business Development Manager", DepartmentId = departmentMap["Business Development"] },
                new() { Name = "Business Development Representative", DepartmentId = departmentMap["Business Development"] },
                new() { Name = "Partnership Manager", DepartmentId = departmentMap["Business Development"] },
                new() { Name = "Account Manager", DepartmentId = departmentMap["Account Management"] },
                new() { Name = "Key Account Manager", DepartmentId = departmentMap["Account Management"] },
                new() { Name = "Client Relationship Manager", DepartmentId = departmentMap["Account Management"] },
                new() { Name = "Product Manager", DepartmentId = departmentMap["Product Management"] },
                new() { Name = "Senior Product Manager", DepartmentId = departmentMap["Product Management"] },
                new() { Name = "Product Owner", DepartmentId = departmentMap["Product Management"] },
                new() { Name = "Product Director", DepartmentId = departmentMap["Product Management"] },
                new() { Name = "Frontend Developer", DepartmentId = departmentMap["Engineering"] },
                new() { Name = "Backend Developer", DepartmentId = departmentMap["Engineering"] },
                new() { Name = "Full Stack Developer", DepartmentId = departmentMap["Engineering"] },
                new() { Name = "Engineering Manager", DepartmentId = departmentMap["Engineering"] },
                new() { Name = "Lead Engineer", DepartmentId = departmentMap["Engineering"] },
                new() { Name = "Principal Engineer", DepartmentId = departmentMap["Engineering"] },
                new() { Name = "Research Scientist", DepartmentId = departmentMap["Research & Development"] },
                new() { Name = "R&D Engineer", DepartmentId = departmentMap["Research & Development"] },
                new() { Name = "Innovation Manager", DepartmentId = departmentMap["Research & Development"] },
                new() { Name = "R&D Director", DepartmentId = departmentMap["Research & Development"] },
                new() { Name = "QA Engineer", DepartmentId = departmentMap["Quality Assurance"] },
                new() { Name = "QA Manager", DepartmentId = departmentMap["Quality Assurance"] },
                new() { Name = "Test Automation Engineer", DepartmentId = departmentMap["Quality Assurance"] },
                new() { Name = "Quality Control Specialist", DepartmentId = departmentMap["Quality Assurance"] },
                new() { Name = "DevOps Engineer", DepartmentId = departmentMap["DevOps"] },
                new() { Name = "Site Reliability Engineer", DepartmentId = departmentMap["DevOps"] },
                new() { Name = "Infrastructure Engineer", DepartmentId = departmentMap["DevOps"] },
                new() { Name = "DevOps Manager", DepartmentId = departmentMap["DevOps"] },
                new() { Name = "Data Scientist", DepartmentId = departmentMap["Data Science"] },
                new() { Name = "Data Analyst", DepartmentId = departmentMap["Data Science"] },
                new() { Name = "Machine Learning Engineer", DepartmentId = departmentMap["Data Science"] },
                new() { Name = "Data Engineer", DepartmentId = departmentMap["Data Science"] },
                new() { Name = "Senior Data Scientist", DepartmentId = departmentMap["Data Science"] },
                new() { Name = "Corporate Lawyer", DepartmentId = departmentMap["Legal"] },
                new() { Name = "Legal Counsel", DepartmentId = departmentMap["Legal"] },
                new() { Name = "Compliance Officer", DepartmentId = departmentMap["Legal"] },
                new() { Name = "Legal Manager", DepartmentId = departmentMap["Legal"] },
                new() { Name = "Procurement Specialist", DepartmentId = departmentMap["Procurement"] },
                new() { Name = "Purchasing Manager", DepartmentId = departmentMap["Procurement"] },
                new() { Name = "Vendor Manager", DepartmentId = departmentMap["Procurement"] },
                new() { Name = "Contract Manager", DepartmentId = departmentMap["Procurement"] },
                new() { Name = "Facilities Manager", DepartmentId = departmentMap["Facilities"] },
                new() { Name = "Maintenance Technician", DepartmentId = departmentMap["Facilities"] },
                new() { Name = "Office Manager", DepartmentId = departmentMap["Facilities"] },
                new() { Name = "Building Coordinator", DepartmentId = departmentMap["Facilities"] },
                new() { Name = "Security Officer", DepartmentId = departmentMap["Security"] },
                new() { Name = "Security Manager", DepartmentId = departmentMap["Security"] },
                new() { Name = "Safety Coordinator", DepartmentId = departmentMap["Security"] },
                new() { Name = "Risk Management Specialist", DepartmentId = departmentMap["Security"] },
                new() { Name = "Administrative Assistant", DepartmentId = departmentMap["Administration"] },
                new() { Name = "Office Coordinator", DepartmentId = departmentMap["Administration"] },
                new() { Name = "Executive Secretary", DepartmentId = departmentMap["Administration"] },
                new() { Name = "Administrative Manager", DepartmentId = departmentMap["Administration"] }
            };

            foreach (var title in jobTitles)
            {
                if (!dbContext.JobTitles.Any(jt => jt.Name == title.Name && jt.DepartmentId == title.DepartmentId))
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

            var userSeeds = new List<(User User, List<int> RoleIds)>
            {
                (new User { 
                    FirstName = "Admin", 
                    LastName = "User", 
                    Email = "admin@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "System Administrator").Id 
                }, new List<int> { adminRoleId, managerRoleId, employeeRoleId }),
                
                (new User { 
                    FirstName = "John", 
                    LastName = "Anderson", 
                    Email = "john.anderson@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "CEO").Id 
                }, new List<int> { adminRoleId, managerRoleId, employeeRoleId }),
                
                (new User { 
                    FirstName = "Sarah", 
                    LastName = "Mitchell", 
                    Email = "sarah.mitchell@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "COO").Id 
                }, new List<int> { adminRoleId, managerRoleId, employeeRoleId }),
                
                (new User { 
                    FirstName = "Michael", 
                    LastName = "Thompson", 
                    Email = "michael.thompson@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "CFO").Id 
                }, new List<int> { adminRoleId, managerRoleId, employeeRoleId }),
                
                (new User { 
                    FirstName = "Emily", 
                    LastName = "Davis", 
                    Email = "emily.davis@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Vice President").Id 
                }, new List<int> { adminRoleId, managerRoleId, employeeRoleId }),
                
                (new User { 
                    FirstName = "Robert", 
                    LastName = "Wilson", 
                    Email = "robert.wilson@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Vice President").Id 
                }, new List<int> { managerRoleId, employeeRoleId }),
                
                (new User { 
                    FirstName = "David", 
                    LastName = "Garcia", 
                    Email = "david.garcia@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "IT Manager").Id 
                }, new List<int> { managerRoleId, employeeRoleId }),
                
                (new User { 
                    FirstName = "Jennifer", 
                    LastName = "Martinez", 
                    Email = "jennifer.martinez@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Software Engineer").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "James", 
                    LastName = "Rodriguez", 
                    Email = "james.rodriguez@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Software Engineer").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Lisa", 
                    LastName = "Hernandez", 
                    Email = "lisa.hernandez@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Software Engineer").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Christopher", 
                    LastName = "Lopez", 
                    Email = "christopher.lopez@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Network Administrator").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Brandon", 
                    LastName = "Allen", 
                    Email = "brandon.allen@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "System Administrator").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Connor", 
                    LastName = "Mitchell", 
                    Email = "connor.mitchell@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Engineering Manager").Id 
                }, new List<int> { managerRoleId, employeeRoleId }),
                
                (new User { 
                    FirstName = "Alexis", 
                    LastName = "Carter", 
                    Email = "alexis.carter@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Lead Engineer").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Blake", 
                    LastName = "Roberts", 
                    Email = "blake.roberts@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Principal Engineer").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Morgan", 
                    LastName = "Gomez", 
                    Email = "morgan.gomez@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Frontend Developer").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Cameron", 
                    LastName = "Phillips", 
                    Email = "cameron.phillips@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Backend Developer").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Gregory", 
                    LastName = "James", 
                    Email = "gregory.james@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Sales Director").Id 
                }, new List<int> { managerRoleId, employeeRoleId }),
                
                (new User { 
                    FirstName = "Victoria", 
                    LastName = "Watson", 
                    Email = "victoria.watson@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Sales Manager").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Adam", 
                    LastName = "Brooks", 
                    Email = "adam.brooks@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Sales Representative").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Emma", 
                    LastName = "Price", 
                    Email = "emma.price@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Sales Representative").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Charlotte", 
                    LastName = "Perry", 
                    Email = "charlotte.perry@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Marketing Director").Id 
                }, new List<int> { managerRoleId, employeeRoleId }),
                
                (new User { 
                    FirstName = "Henry", 
                    LastName = "Powell", 
                    Email = "henry.powell@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Marketing Manager").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Amelia", 
                    LastName = "Long", 
                    Email = "amelia.long@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Brand Manager").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Ethan", 
                    LastName = "Flores", 
                    Email = "ethan.flores@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Content Creator").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Lucas", 
                    LastName = "Butler", 
                    Email = "lucas.butler@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "HR Director").Id 
                }, new List<int> { managerRoleId, employeeRoleId }),
                
                (new User { 
                    FirstName = "Abigail", 
                    LastName = "Simmons", 
                    Email = "abigail.simmons@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "HR Manager").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Gabriel", 
                    LastName = "Foster", 
                    Email = "gabriel.foster@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "HR Business Partner").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Scarlett", 
                    LastName = "Alexander", 
                    Email = "scarlett.alexander@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Recruiter").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Owen", 
                    LastName = "Russell", 
                    Email = "owen.russell@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Finance Manager").Id 
                }, new List<int> { managerRoleId, employeeRoleId }),
                
                (new User { 
                    FirstName = "Grace", 
                    LastName = "Griffin", 
                    Email = "grace.griffin@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Controller").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Sebastian", 
                    LastName = "Diaz", 
                    Email = "sebastian.diaz@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Financial Analyst").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Jack", 
                    LastName = "Myers", 
                    Email = "jack.myers@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Accountant").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Zoey", 
                    LastName = "Graham", 
                    Email = "zoey.graham@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Customer Service Manager").Id 
                }, new List<int> { managerRoleId, employeeRoleId }),
                
                (new User { 
                    FirstName = "Luke", 
                    LastName = "Sullivan", 
                    Email = "luke.sullivan@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Customer Success Manager").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Layla", 
                    LastName = "Wallace", 
                    Email = "layla.wallace@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Customer Service Representative").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Wyatt", 
                    LastName = "West", 
                    Email = "wyatt.west@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Operations Director").Id 
                }, new List<int> { managerRoleId, employeeRoleId }),
                
                (new User { 
                    FirstName = "Nora", 
                    LastName = "Tucker", 
                    Email = "nora.tucker@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Operations Manager").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Leo", 
                    LastName = "Parker", 
                    Email = "leo.parker@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Operations Analyst").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Alex", 
                    LastName = "Rogers", 
                    Email = "alex.rogers@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Senior Data Scientist").Id 
                }, new List<int> { managerRoleId, employeeRoleId }),
                
                (new User { 
                    FirstName = "Vanessa", 
                    LastName = "Reed", 
                    Email = "vanessa.reed@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Data Scientist").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Carlos", 
                    LastName = "Bailey", 
                    Email = "carlos.bailey@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Data Analyst").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Isaiah", 
                    LastName = "Thompson", 
                    Email = "isaiah.thompson@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "QA Manager").Id 
                }, new List<int> { managerRoleId, employeeRoleId }),
                
                (new User { 
                    FirstName = "Aurora", 
                    LastName = "White", 
                    Email = "aurora.white@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "QA Engineer").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Grayson", 
                    LastName = "Martinez", 
                    Email = "grayson.martinez@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "DevOps Manager").Id 
                }, new List<int> { managerRoleId, employeeRoleId }),
                
                (new User { 
                    FirstName = "Penelope", 
                    LastName = "Anderson", 
                    Email = "penelope.anderson@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "DevOps Engineer").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Adrian", 
                    LastName = "Moore", 
                    Email = "adrian.moore@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Product Director").Id 
                }, new List<int> { managerRoleId, employeeRoleId }),
                
                (new User { 
                    FirstName = "Addison", 
                    LastName = "Martin", 
                    Email = "addison.martin@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Senior Product Manager").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Lincoln", 
                    LastName = "Garcia", 
                    Email = "lincoln.garcia@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Legal Manager").Id 
                }, new List<int> { managerRoleId, employeeRoleId }),
                
                (new User { 
                    FirstName = "Elena", 
                    LastName = "Rodriguez", 
                    Email = "elena.rodriguez@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Corporate Lawyer").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Mateo", 
                    LastName = "Flores", 
                    Email = "mateo.flores@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "R&D Director").Id 
                }, new List<int> { managerRoleId, employeeRoleId }),
                
                (new User { 
                    FirstName = "Genesis", 
                    LastName = "Green", 
                    Email = "genesis.green@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Research Scientist").Id 
                }, new List<int> { employeeRoleId }),
                
                (new User { 
                    FirstName = "Serenity", 
                    LastName = "Nelson", 
                    Email = "serenity.nelson@simulator.com", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"), 
                    JobTitleId = jobTitlesList.First(jt => jt.Name == "Innovation Manager").Id 
                }, new List<int> { adminRoleId, employeeRoleId })
            };

            foreach (var (user, roleIds) in userSeeds)
            {
                if (!dbContext.Users.Any(u => u.Email == user.Email))
                {
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

            var rnd = new Random();
            for (int i = 1; i <= 500; i++)
            {
                var email = $"test{i}@simulator.com";
                if (dbContext.Users.Any(u => u.Email == email))
                    continue;

                var job = jobTitlesList[rnd.Next(jobTitlesList.Count)];
                var user = new User
                {
                    FirstName = $"TestF{i}",
                    LastName = $"TestL{i}",
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass123"),
                    JobTitleId = job.Id
                };

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
                    RequestStatus = (ManagementSimulator.Database.Enums.RequestStatus)leaveStatuses.GetValue(random2.Next(leaveStatuses.Length)),
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
        }
    }
}