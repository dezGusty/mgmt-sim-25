using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories;
using ManagementSimulator.Tests;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace ManagementSimulator.Tests.Repositories
{
    public class SecondManagerRepositoryTests : IDisposable
    {
        private readonly MGMTSimulatorDbContext _context;
        private readonly SecondManagerRepository _repository;

        public SecondManagerRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<MGMTSimulatorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MGMTSimulatorDbContext(options);
            _repository = new SecondManagerRepository(_context);
        }

        [Fact]
        public async Task AddSecondManagerAsync_ShouldAddSecondManager()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Manager" };

            var employee = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var manager = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Departments.Add(department);
            _context.JobTitles.Add(jobTitle);
            _context.Users.AddRange(employee, manager);
            await _context.SaveChangesAsync();

            var secondManager = new SecondManager
            {
                SecondManagerEmployeeId = employee.Id,
                ReplacedManagerId = manager.Id,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1)
            };

            // Act
            await _repository.AddSecondManagerAsync(secondManager);
            var result = await _context.SecondManagers.FirstOrDefaultAsync(sm => sm.SecondManagerEmployeeId == employee.Id && sm.ReplacedManagerId == manager.Id);

            // Assert
            result.Should().NotBeNull();
            result!.SecondManagerEmployeeId.Should().Be(employee.Id);
            result.ReplacedManagerId.Should().Be(manager.Id);
        }

        [Fact]
        public async Task GetAllSecondManagersAsync_ShouldReturnAllSecondManagers()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Manager" };

            var user1 = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var user2 = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var user3 = new User
            {
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob.johnson@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var user4 = new User
            {
                FirstName = "Alice",
                LastName = "Brown",
                Email = "alice.brown@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Departments.Add(department);
            _context.JobTitles.Add(jobTitle);
            _context.Users.AddRange(user1, user2, user3, user4);
            await _context.SaveChangesAsync();

            var secondManager1 = new SecondManager
            {
                SecondManagerEmployeeId = user1.Id,
                ReplacedManagerId = user2.Id,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1)
            };

            var secondManager2 = new SecondManager
            {
                SecondManagerEmployeeId = user3.Id,
                ReplacedManagerId = user4.Id,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1)
            };

            var secondManager3 = new SecondManager
            {
                SecondManagerEmployeeId = user3.Id,
                ReplacedManagerId = user4.Id,
                StartDate = DateTime.Now.AddDays(5),
                EndDate = DateTime.Now.AddMonths(1)
            };

            _context.SecondManagers.AddRange(secondManager1, secondManager2, secondManager3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllSecondManagersAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(sm => sm.SecondManagerEmployeeId == user1.Id && sm.ReplacedManagerId == user2.Id);
            result.Should().Contain(sm => sm.SecondManagerEmployeeId == user3.Id && sm.ReplacedManagerId == user4.Id);
        }

        [Fact]

        public async Task GetActiveSecondManagersAsync_ShouldReturnOnlyActiveSM()
        {
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Manager" };

            var user1 = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var user2 = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Departments.Add(department);
            _context.JobTitles.Add(jobTitle);
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var activeSecondManager = new SecondManager
            {
                SecondManagerEmployeeId = user1.Id,
                ReplacedManagerId = user2.Id,
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddDays(1)
            };

            var inactiveSecondManager1 = new SecondManager
            {
                SecondManagerEmployeeId = user1.Id,
                ReplacedManagerId = user2.Id,
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddDays(-1)
            };

            var inactiveSecondManager2 = new SecondManager
            {
                SecondManagerEmployeeId = user1.Id,
                ReplacedManagerId = user2.Id,
                StartDate = DateTime.Now.AddDays(2),
                EndDate = DateTime.Now.AddMonths(1)
            };

            _context.SecondManagers.AddRange(activeSecondManager, inactiveSecondManager1, inactiveSecondManager2);
            await _context.SaveChangesAsync();

            var result = await _repository.GetActiveSecondManagersAsync();

            result.Should().HaveCount(1);
            result[0].Should().BeEquivalentTo(activeSecondManager);
        }

        [Fact]

        public async Task GetSecondManagersByReplacedManagerIdAsync_ShouldReturnCorrectSM()
        {
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Manager" };

            var user1 = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var user2 = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Departments.Add(department);
            _context.JobTitles.Add(jobTitle);
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var activeSecondManager = new SecondManager
            {
                SecondManagerEmployeeId = user1.Id,
                ReplacedManagerId = user2.Id,
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddDays(1)
            };

            var inactiveSecondManager1 = new SecondManager
            {
                SecondManagerEmployeeId = user1.Id,
                ReplacedManagerId = user2.Id,
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddDays(-1)
            };

            _context.SecondManagers.AddRange(activeSecondManager, inactiveSecondManager1);
            await _context.SaveChangesAsync();

            var result = await _repository.GetSecondManagersByReplacedManagerIdAsync(user2.Id);

            result.Should().HaveCount(2);
            result.All(sm => sm.ReplacedManagerId == user2.Id).Should().BeTrue();
        }

        [Fact]
        public async Task GetSecondManagersByReplacedManagerIdAsync_ShouldReturnEmptyList_WhenNoSMFound()
        {
            var result = await _repository.GetSecondManagersByReplacedManagerIdAsync(999);

            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData(-30, 5)]
        [InlineData(-30, -5)]
        [InlineData(5, 30)]
        public async Task GetSecondManagerAsync_ShouldReturnCorrectSM(int startDaysOffset, int endDaysOffset)
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Manager" };

            var user1 = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var user2 = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Departments.Add(department);
            _context.JobTitles.Add(jobTitle);
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var startDate = DateTime.Now.AddDays(startDaysOffset);
            var endDate = DateTime.Now.AddDays(endDaysOffset);

            var secondManager = new SecondManager
            {
                SecondManagerEmployeeId = user1.Id,
                ReplacedManagerId = user2.Id,
                StartDate = startDate,
                EndDate = endDate
            };

            _context.SecondManagers.Add(secondManager);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetSecondManagerAsync(user1.Id, user2.Id, startDate);

            // Assert
            result.Should().NotBeNull();
            result!.SecondManagerEmployeeId.Should().Be(user1.Id);
            result.ReplacedManagerId.Should().Be(user2.Id);
            result.StartDate.Should().Be(startDate);
            result.EndDate.Should().Be(endDate);
        }

        [Fact]
        public async Task GetSecondManagerAsync_ShouldReturnNull_WhenNotFound()
        {
            var result = await _repository.GetSecondManagerAsync(999, 888, DateTime.Now);

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateSecondManager_ShouldUpdate()
        {
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Manager" };

            var user1 = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var user2 = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Departments.Add(department);
            _context.JobTitles.Add(jobTitle);
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var secondManager = new SecondManager
            {
                SecondManagerEmployeeId = user1.Id,
                ReplacedManagerId = user2.Id,
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddDays(1)
            };

            _context.SecondManagers.Add(secondManager);
            await _context.SaveChangesAsync();

            secondManager.EndDate = DateTime.Now.AddMonths(2);
            await _repository.UpdateSecondManagerAsync(secondManager);

            var result = await _repository.GetSecondManagerAsync(user1.Id, user2.Id, secondManager.StartDate);

            result.Should().NotBeNull();
            result!.EndDate.Should().Be(secondManager.EndDate);
        }

        [Fact]
        public async Task DeleteSecondManagerAsync_ShouldDeleteExistingSecondManager()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Manager" };

            var user1 = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var user2 = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Departments.Add(department);
            _context.JobTitles.Add(jobTitle);
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var startDate = DateTime.Now.AddMonths(-1);
            var secondManager = new SecondManager
            {
                SecondManagerEmployeeId = user1.Id,
                ReplacedManagerId = user2.Id,
                StartDate = startDate,
                EndDate = DateTime.Now.AddDays(1)
            };

            _context.SecondManagers.Add(secondManager);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteSecondManagerAsync(user1.Id, user2.Id, startDate);

            // Assert
            var result = await _repository.GetSecondManagerAsync(user1.Id, user2.Id, startDate);
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteSecondManagerAsync_ShouldDoNothing_WhenSecondManagerDoesNotExist()
        {
            // Arrange & Act
            await _repository.DeleteSecondManagerAsync(999, 888, DateTime.Now);

            // Assert
            var allSecondManagers = await _repository.GetAllSecondManagersAsync();
            allSecondManagers.Should().BeEmpty();
        }

        [Fact]
        public async Task GetEmployeesForActiveSecondManagerAsync_ShouldReturnEmployeesForActiveReplacements()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Manager" };

            var secondManagerEmployee = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var replacedManager = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var employee1 = new User
            {
                FirstName = "Alice",
                LastName = "Brown",
                Email = "alice.brown@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var employee2 = new User
            {
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob.johnson@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Departments.Add(department);
            _context.JobTitles.Add(jobTitle);
            _context.Users.AddRange(secondManagerEmployee, replacedManager, employee1, employee2);
            await _context.SaveChangesAsync();

            var activeSecondManager = new SecondManager
            {
                SecondManagerEmployeeId = secondManagerEmployee.Id,
                ReplacedManagerId = replacedManager.Id,
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddMonths(1)
            };

            _context.SecondManagers.Add(activeSecondManager);

            var employeeManager1 = new EmployeeManager
            {
                EmployeeId = employee1.Id,
                ManagerId = replacedManager.Id
            };

            var employeeManager2 = new EmployeeManager
            {
                EmployeeId = employee2.Id,
                ManagerId = replacedManager.Id
            };

            _context.EmployeeManagers.AddRange(employeeManager1, employeeManager2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetEmployeesForActiveSecondManagerAsync(secondManagerEmployee.Id);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(e => e.Id == employee1.Id);
            result.Should().Contain(e => e.Id == employee2.Id);
        }

        [Fact]
        public async Task GetEmployeesForActiveSecondManagerAsync_ShouldReturnEmpty_WhenNoActiveReplacements()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Manager" };

            var secondManagerEmployee = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var replacedManager = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Departments.Add(department);
            _context.JobTitles.Add(jobTitle);
            _context.Users.AddRange(secondManagerEmployee, replacedManager);
            await _context.SaveChangesAsync();

            var inactiveSecondManager = new SecondManager
            {
                SecondManagerEmployeeId = secondManagerEmployee.Id,
                ReplacedManagerId = replacedManager.Id,
                StartDate = DateTime.Now.AddMonths(-2),
                EndDate = DateTime.Now.AddDays(-1)
            };

            var futureSecondManager = new SecondManager
            {
                SecondManagerEmployeeId = secondManagerEmployee.Id,
                ReplacedManagerId = replacedManager.Id,
                StartDate = DateTime.Now.AddDays(1),
                EndDate = DateTime.Now.AddMonths(2)
            };

            _context.SecondManagers.AddRange(inactiveSecondManager, futureSecondManager);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetEmployeesForActiveSecondManagerAsync(secondManagerEmployee.Id);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetEmployeesForActiveSecondManagerAsync_ShouldReturnEmpty_WhenUserIsNotSecondManager()
        {
            // Act
            var result = await _repository.GetEmployeesForActiveSecondManagerAsync(999);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetEmployeesForActiveSecondManagerAsync_ShouldReturnDistinctEmployees()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Manager" };

            var secondManagerEmployee = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var replacedManager1 = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var replacedManager2 = new User
            {
                FirstName = "Mike",
                LastName = "Wilson",
                Email = "mike.wilson@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var employee = new User
            {
                FirstName = "Alice",
                LastName = "Brown",
                Email = "alice.brown@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Departments.Add(department);
            _context.JobTitles.Add(jobTitle);
            _context.Users.AddRange(secondManagerEmployee, replacedManager1, replacedManager2, employee);
            await _context.SaveChangesAsync();

            var activeSecondManager1 = new SecondManager
            {
                SecondManagerEmployeeId = secondManagerEmployee.Id,
                ReplacedManagerId = replacedManager1.Id,
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddMonths(1)
            };

            var activeSecondManager2 = new SecondManager
            {
                SecondManagerEmployeeId = secondManagerEmployee.Id,
                ReplacedManagerId = replacedManager2.Id,
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddMonths(1)
            };

            _context.SecondManagers.AddRange(activeSecondManager1, activeSecondManager2);
            await _context.SaveChangesAsync();

            var employeeManager1 = new EmployeeManager
            {
                EmployeeId = employee.Id,
                ManagerId = replacedManager1.Id
            };

            var employeeManager2 = new EmployeeManager
            {
                EmployeeId = employee.Id,
                ManagerId = replacedManager2.Id
            };

            _context.EmployeeManagers.AddRange(employeeManager1, employeeManager2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetEmployeesForActiveSecondManagerAsync(secondManagerEmployee.Id);

            // Assert
            result.Should().HaveCount(1);
            result.Should().Contain(e => e.Id == employee.Id);
        }

        [Fact]
        public async Task GetAllSecondManagersAsync_ShouldReturnEmpty_WhenNoSecondManagers()
        {
            // Act
            var result = await _repository.GetAllSecondManagersAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetActiveSecondManagersAsync_ShouldReturnEmpty_WhenNoActiveSecondManagers()
        {
            // Act
            var result = await _repository.GetActiveSecondManagersAsync();

            // Assert
            result.Should().BeEmpty();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}