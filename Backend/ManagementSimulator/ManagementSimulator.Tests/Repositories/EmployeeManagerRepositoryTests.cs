using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories;
using ManagementSimulator.Tests;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace ManagementSimulator.Tests.Repositories
{
    public class EmployeeManagerRepositoryTests : IDisposable
    {
        private readonly MGMTSimulatorDbContext _context;
        private readonly EmployeeManagerRepository _repository;

        public EmployeeManagerRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<MGMTSimulatorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MGMTSimulatorDbContext(options);
            _repository = new EmployeeManagerRepository(_context);
        }

        [Fact]
        public async Task AddEmployeeManagersAsync_ShouldCreateEmployeeManagerRelationship()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };

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

            // Act
            await _repository.AddEmployeeManagersAsync(employee.Id, manager.Id);

            // Assert
            var relationship = await _context.EmployeeManagers
                .FirstOrDefaultAsync(em => em.EmployeeId == employee.Id && em.ManagerId == manager.Id);

            relationship.Should().NotBeNull();
            relationship!.EmployeeId.Should().Be(employee.Id);
            relationship.ManagerId.Should().Be(manager.Id);
            relationship.DeletedAt.Should().BeNull();
        }

        [Fact]
        public async Task DeleteEmployeeManagerAsync_ShouldSoftDeleteRelationship()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };

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

            var employeeManager = new EmployeeManager
            {
                EmployeeId = employee.Id,
                ManagerId = manager.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmployeeManagers.Add(employeeManager);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteEmployeeManagerAsync(employee.Id, manager.Id);

            // Assert
            var relationship = await _context.EmployeeManagers
                .FirstOrDefaultAsync(em => em.EmployeeId == employee.Id && em.ManagerId == manager.Id);

            relationship.Should().NotBeNull();
            relationship!.DeletedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task GetManagersForEmployeesByIdAsync_ShouldReturnActiveManagers()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };
            var managerTitle = new JobTitle { Name = "Manager" };

            var employee = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var manager1 = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                PasswordHash = "hash",
                JobTitleId = 2,
                DepartmentId = 1
            };

            var manager2 = new User
            {
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob.johnson@test.com",
                PasswordHash = "hash",
                JobTitleId = 2,
                DepartmentId = 1
            };

            _context.Departments.Add(department);
            _context.JobTitles.AddRange(jobTitle, managerTitle);
            _context.Users.AddRange(employee, manager1, manager2);
            await _context.SaveChangesAsync();

            var employeeManager1 = new EmployeeManager
            {
                EmployeeId = employee.Id,
                ManagerId = manager1.Id,
                CreatedAt = DateTime.UtcNow
            };

            var employeeManager2 = new EmployeeManager
            {
                EmployeeId = employee.Id,
                ManagerId = manager2.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmployeeManagers.AddRange(employeeManager1, employeeManager2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetManagersForEmployeesByIdAsync(employee.Id);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(m => m.Id == manager1.Id);
            result.Should().Contain(m => m.Id == manager2.Id);
            result.All(m => m.DeletedAt == null).Should().BeTrue();
        }

        [Fact]
        public async Task GetManagersForEmployeesByIdAsync_ShouldExcludeDeletedManagers()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };

            var employee = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var activeManager = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var deletedManager = new User
            {
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob.johnson@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1,
                DeletedAt = DateTime.UtcNow
            };

            _context.Departments.Add(department);
            _context.JobTitles.Add(jobTitle);
            _context.Users.AddRange(employee, activeManager, deletedManager);
            await _context.SaveChangesAsync();

            var activeRelationship = new EmployeeManager
            {
                EmployeeId = employee.Id,
                ManagerId = activeManager.Id,
                CreatedAt = DateTime.UtcNow
            };

            var deletedRelationship = new EmployeeManager
            {
                EmployeeId = employee.Id,
                ManagerId = deletedManager.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmployeeManagers.AddRange(activeRelationship, deletedRelationship);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetManagersForEmployeesByIdAsync(employee.Id);

            // Assert
            result.Should().HaveCount(1);
            result[0].Id.Should().Be(activeManager.Id);
            result[0].DeletedAt.Should().BeNull();
        }

        [Fact]
        public async Task GetEmployeesForManagerByIdAsync_ShouldReturnActiveEmployees()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };

            var manager = new User
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
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var employee2 = new User
            {
                FirstName = "Alice",
                LastName = "Brown",
                Email = "alice.brown@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var deletedEmployee = new User
            {
                FirstName = "Alice",
                LastName = "Brown",
                Email = "alice.brown@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1,
                DeletedAt = DateTime.UtcNow
            };

            _context.Departments.Add(department);
            _context.JobTitles.Add(jobTitle);
            _context.Users.AddRange(manager, employee1, employee2, deletedEmployee);
            await _context.SaveChangesAsync();

            var employeeManager1 = new EmployeeManager
            {
                EmployeeId = employee1.Id,
                ManagerId = manager.Id,
                CreatedAt = DateTime.UtcNow
            };

            var employeeManager2 = new EmployeeManager
            {
                EmployeeId = employee2.Id,
                ManagerId = manager.Id,
                CreatedAt = DateTime.UtcNow
            };

            var deletedEmployeeManager = new EmployeeManager
            {
                EmployeeId = deletedEmployee.Id,
                ManagerId = manager.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmployeeManagers.AddRange(employeeManager1, employeeManager2, deletedEmployeeManager);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetEmployeesForManagerByIdAsync(manager.Id);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(e => e.Id == employee1.Id);
            result.Should().Contain(e => e.Id == employee2.Id);
            result.All(e => e.DeletedAt == null).Should().BeTrue();
        }

        [Fact]
        public async Task GetEMRelationshipForEmployeesByIdAsync_ShouldReturnActiveRelationships()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };

            var employee = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var manager1 = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var manager2 = new User
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
            _context.Users.AddRange(employee, manager1, manager2);
            await _context.SaveChangesAsync();

            var relationship1 = new EmployeeManager
            {
                EmployeeId = employee.Id,
                ManagerId = manager1.Id,
                CreatedAt = DateTime.UtcNow
            };

            var relationship2 = new EmployeeManager
            {
                EmployeeId = employee.Id,
                ManagerId = manager2.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmployeeManagers.AddRange(relationship1, relationship2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetEMRelationshipForEmployeesByIdAsync(employee.Id);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(r => r.ManagerId == manager1.Id);
            result.Should().Contain(r => r.ManagerId == manager2.Id);
            result.All(r => r.EmployeeId == employee.Id).Should().BeTrue();
            result.All(r => r.DeletedAt == null).Should().BeTrue();
        }

        [Fact]
        public async Task GetEMRelationshipForEmployeesByIdAsync_ShouldExcludeDeletedByDefault()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };

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

            var activeRelationship = new EmployeeManager
            {
                EmployeeId = employee.Id,
                ManagerId = manager.Id,
                CreatedAt = DateTime.UtcNow
            };

            var deletedRelationship = new EmployeeManager
            {
                EmployeeId = employee.Id,
                ManagerId = manager.Id + 100,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                DeletedAt = DateTime.UtcNow
            };

            _context.EmployeeManagers.AddRange(activeRelationship, deletedRelationship);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetEMRelationshipForEmployeesByIdAsync(employee.Id);

            // Assert
            result.Should().HaveCount(1);
            result[0].ManagerId.Should().Be(manager.Id);
            result[0].DeletedAt.Should().BeNull();
        }

        [Fact]
        public async Task GetEMRelationshipForEmployeesByIdAsync_ShouldIncludeDeletedWhenRequested()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };

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

            var activeRelationship = new EmployeeManager
            {
                EmployeeId = employee.Id,
                ManagerId = manager.Id,
                CreatedAt = DateTime.UtcNow
            };

            var deletedRelationship = new EmployeeManager
            {
                EmployeeId = employee.Id,
                ManagerId = manager.Id + 100,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                DeletedAt = DateTime.UtcNow
            };

            _context.EmployeeManagers.AddRange(activeRelationship, deletedRelationship);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetEMRelationshipForEmployeesByIdAsync(employee.Id, includeDeleted: true);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(r => r.DeletedAt == null);
            result.Should().Contain(r => r.DeletedAt != null);
        }

        [Fact]
        public async Task GetEmployeeManagersByIdAsync_ShouldReturnSpecificRelationship()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };

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

            var relationship = new EmployeeManager
            {
                EmployeeId = employee.Id,
                ManagerId = manager.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmployeeManagers.Add(relationship);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetEmployeeManagersByIdAsync(employee.Id, manager.Id);

            // Assert
            result.Should().NotBeNull();
            result!.EmployeeId.Should().Be(employee.Id);
            result.ManagerId.Should().Be(manager.Id);
            result.DeletedAt.Should().BeNull();
        }

        [Fact]
        public async Task GetEmployeeManagersByIdAsync_ShouldReturnNullWhenNotFound()
        {
            // Act
            var result = await _repository.GetEmployeeManagersByIdAsync(999, 888);

            // Assert
            result.Should().BeNull();
        }


        [Fact]
        public async Task GetEmployeeManagersByIdAsync_ShouldIncludeDeletedWhenRequested()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };

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

            var deletedRelationship = new EmployeeManager
            {
                EmployeeId = employee.Id,
                ManagerId = manager.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                DeletedAt = DateTime.UtcNow
            };

            _context.EmployeeManagers.Add(deletedRelationship);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetEmployeeManagersByIdAsync(employee.Id, manager.Id, includeDeleted: true);

            // Assert
            result.Should().NotBeNull();
            result!.EmployeeId.Should().Be(employee.Id);
            result.ManagerId.Should().Be(manager.Id);
            result.DeletedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllEmployeeManagersAsync_ShouldReturnAllActiveRelationships()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };

            var employee1 = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            var employee2 = new User
            {
                FirstName = "Alice",
                LastName = "Brown",
                Email = "alice.brown@test.com",
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
            _context.Users.AddRange(employee1, employee2, manager);
            await _context.SaveChangesAsync();

            var relationship1 = new EmployeeManager
            {
                EmployeeId = employee1.Id,
                ManagerId = manager.Id,
                CreatedAt = DateTime.UtcNow
            };

            var relationship2 = new EmployeeManager
            {
                EmployeeId = employee2.Id,
                ManagerId = manager.Id,
                CreatedAt = DateTime.UtcNow
            };

            var deletedRelationship = new EmployeeManager
            {
                EmployeeId = employee1.Id,
                ManagerId = manager.Id + 100,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                DeletedAt = DateTime.UtcNow
            };

            _context.EmployeeManagers.AddRange(relationship1, relationship2, deletedRelationship);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllEmployeeManagersAsync();

            // Assert
            result.Should().HaveCount(2);
            result.All(r => r.DeletedAt == null).Should().BeTrue();
            result.Should().Contain(r => r.EmployeeId == employee1.Id && r.ManagerId == manager.Id);
            result.Should().Contain(r => r.EmployeeId == employee2.Id && r.ManagerId == manager.Id);
        }

        [Fact]
        public async Task GetAllEmployeeManagersAsync_ShouldIncludeDeletedWhenRequested()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };

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

            var activeRelationship = new EmployeeManager
            {
                EmployeeId = employee.Id,
                ManagerId = manager.Id,
                CreatedAt = DateTime.UtcNow
            };

            var deletedRelationship = new EmployeeManager
            {
                EmployeeId = employee.Id,
                ManagerId = manager.Id + 100,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                DeletedAt = DateTime.UtcNow
            };

            _context.EmployeeManagers.AddRange(activeRelationship, deletedRelationship);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllEmployeeManagersAsync(includeDeleted: true);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(r => r.DeletedAt == null);
            result.Should().Contain(r => r.DeletedAt != null);
        }

        [Fact]
        public async Task SetSubordinatesToUnassignedByManagerIdAsync_ShouldThrowExceptionWithInMemoryDatabase()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };

            var employee1 = new User
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
            _context.Users.AddRange(employee1, manager);
            await _context.SaveChangesAsync();

            var relationship1 = new EmployeeManager
            {
                EmployeeId = employee1.Id,
                ManagerId = manager.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmployeeManagers.Add(relationship1);
            await _context.SaveChangesAsync();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _repository.SetSubordinatesToUnassignedByManagerIdAsync(manager.Id));

            exception.Message.Should().Contain("ExecuteUpdate");
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
