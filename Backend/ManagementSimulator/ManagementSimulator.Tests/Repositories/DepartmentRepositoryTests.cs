using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories;
using ManagementSimulator.Database.Dtos.QueryParams;
using ManagementSimulator.Tests;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace ManagementSimulator.Tests.Repositories
{
    public class DepartmentRepositoryTests : IDisposable
    {
        private readonly MGMTSimulatorDbContext _context;
        private readonly DepartmentRepository _repository;

        public DepartmentRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<MGMTSimulatorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MGMTSimulatorDbContext(options);
            _repository = new DepartmentRepository(_context, new TestAuditService());
        }

        [Fact]
        public async Task GetDepartmentByNameAsync_ShouldReturnDepartmentWhenFound()
        {
            // Arrange
            var department = new Department
            {
                Name = "Information Technology",
                Description = "IT Department handling all technology needs"
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetDepartmentByNameAsync("Information Technology");

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Information Technology");
            result.Description.Should().Be("IT Department handling all technology needs");
        }

        [Fact]
        public async Task GetDepartmentByNameAsync_ShouldReturnNullWhenNotFound()
        {
            // Arrange
            var department = new Department
            {
                Name = "Human Resources",
                Description = "HR Department"
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetDepartmentByNameAsync("Non Existent Department");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetDepartmentByNameAsync_ShouldExcludeDeletedByDefault()
        {
            // Arrange
            var deletedDepartment = new Department
            {
                Name = "Deleted Department",
                Description = "This department was deleted",
                DeletedAt = DateTime.UtcNow
            };

            _context.Departments.Add(deletedDepartment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetDepartmentByNameAsync("Deleted Department");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetDepartmentByNameAsync_ShouldIncludeDeletedWhenRequested()
        {
            // Arrange
            var deletedDepartment = new Department
            {
                Name = "Deleted Department",
                Description = "This department was deleted",
                DeletedAt = DateTime.UtcNow
            };

            _context.Departments.Add(deletedDepartment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetDepartmentByNameAsync("Deleted Department", includeDeleted: true);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Deleted Department");
            result.DeletedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDepartmentByIdAsync_ShouldReturnDepartmentWhenFound()
        {
            // Arrange
            var department = new Department
            {
                Name = "Finance",
                Description = "Finance Department managing budgets"
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetDepartmentByIdAsync(department.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(department.Id);
            result.Name.Should().Be("Finance");
            result.Description.Should().Be("Finance Department managing budgets");
        }

        [Fact]
        public async Task GetDepartmentByIdAsync_ShouldReturnNullWhenNotFound()
        {
            // Act
            var result = await _repository.GetDepartmentByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetDepartmentByIdAsync_ShouldExcludeDeletedByDefault()
        {
            // Arrange
            var deletedDepartment = new Department
            {
                Name = "Deleted Department",
                Description = "This department was deleted",
                DeletedAt = DateTime.UtcNow
            };

            _context.Departments.Add(deletedDepartment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetDepartmentByIdAsync(deletedDepartment.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetDepartmentByIdAsync_ShouldIncludeDeletedWhenRequested()
        {
            // Arrange
            var deletedDepartment = new Department
            {
                Name = "Deleted Department",
                Description = "This department was deleted",
                DeletedAt = DateTime.UtcNow
            };

            _context.Departments.Add(deletedDepartment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetDepartmentByIdAsync(deletedDepartment.Id, includeDeleted: true);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(deletedDepartment.Id);
            result.DeletedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllDepartmentsAsync_ShouldReturnDepartmentsByIds()
        {
            // Arrange
            var department1 = new Department { Name = "IT", Description = "Information Technology" };
            var department2 = new Department { Name = "HR", Description = "Human Resources" };
            var department3 = new Department { Name = "Finance", Description = "Finance Department" };

            _context.Departments.AddRange(department1, department2, department3);
            await _context.SaveChangesAsync();

            var requestedIds = new List<int> { department1.Id, department3.Id };

            // Act
            var result = await _repository.GetAllDepartmentsAsync(requestedIds);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(d => d.Id == department1.Id);
            result.Should().Contain(d => d.Id == department3.Id);
            result.Should().NotContain(d => d.Id == department2.Id);
        }

        [Fact]
        public async Task GetAllDepartmentsAsync_ShouldExcludeDeletedByDefault()
        {
            // Arrange
            var activeDepartment = new Department { Name = "IT", Description = "Information Technology" };
            var deletedDepartment = new Department 
            { 
                Name = "Closed Dept", 
                Description = "This department was closed",
                DeletedAt = DateTime.UtcNow
            };

            _context.Departments.AddRange(activeDepartment, deletedDepartment);
            await _context.SaveChangesAsync();

            var requestedIds = new List<int> { activeDepartment.Id, deletedDepartment.Id };

            // Act
            var result = await _repository.GetAllDepartmentsAsync(requestedIds);

            // Assert
            result.Should().HaveCount(1);
            result[0].Id.Should().Be(activeDepartment.Id);
            result[0].DeletedAt.Should().BeNull();
        }

        [Fact]
        public async Task GetAllDepartmentsFilteredAsync_ShouldReturnAllActiveDepartments()
        {
            // Arrange
            var jobTitle = new JobTitle { Name = "Developer" };
            _context.JobTitles.Add(jobTitle);
            await _context.SaveChangesAsync();

            var department1 = new Department { Name = "IT", Description = "Information Technology" };
            var department2 = new Department { Name = "HR", Description = "Human Resources" };
            var deletedDepartment = new Department 
            { 
                Name = "Closed", 
                Description = "Closed department",
                DeletedAt = DateTime.UtcNow
            };

            _context.Departments.AddRange(department1, department2, deletedDepartment);
            await _context.SaveChangesAsync();

            // Add users to departments to test employee count
            var user1 = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = jobTitle.Id,
                DepartmentId = department1.Id
            };
            var user2 = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                PasswordHash = "hash",
                JobTitleId = jobTitle.Id,
                DepartmentId = department1.Id
            };

            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams();

            // Act
            var result = await _repository.GetAllDepartmentsFilteredAsync(null, parameters);

            // Assert
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.Data.Should().Contain(d => d.Name == "IT");
            result.Data.Should().Contain(d => d.Name == "HR");
            result.Data.Should().NotContain(d => d.Name == "Closed");
            
            var itDept = result.Data.First(d => d.Name == "IT");
            itDept.EmployeeCount.Should().Be(2);
        }

        [Fact]
        public async Task GetAllDepartmentsFilteredAsync_ShouldFilterByName()
        {
            // Arrange
            var department1 = new Department { Name = "Information Technology", Description = "IT Dept" };
            var department2 = new Department { Name = "Human Resources", Description = "HR Dept" };
            var department3 = new Department { Name = "Information Security", Description = "InfoSec Dept" };

            _context.Departments.AddRange(department1, department2, department3);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams();

            // Act
            var result = await _repository.GetAllDepartmentsFilteredAsync("Information", parameters);

            // Assert
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.Data.Should().Contain(d => d.Name == "Information Technology");
            result.Data.Should().Contain(d => d.Name == "Information Security");
            result.Data.Should().NotContain(d => d.Name == "Human Resources");
        }

        [Fact]
        public async Task GetAllDepartmentsFilteredAsync_ShouldApplyPagination()
        {
            // Arrange
            var departments = new List<Department>();
            for (int i = 1; i <= 5; i++)
            {
                departments.Add(new Department
                {
                    Name = $"Department {i}",
                    Description = $"Description for Department {i}"
                });
            }

            _context.Departments.AddRange(departments);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams
            {
                Page = 2,
                PageSize = 2
            };

            // Act
            var result = await _repository.GetAllDepartmentsFilteredAsync(null, parameters);

            // Assert
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(5);
            result.Data[0].Name.Should().Be("Department 3");
            result.Data[1].Name.Should().Be("Department 4");
        }

        [Fact]
        public async Task GetAllDepartmentsFilteredAsync_ShouldApplySorting()
        {
            // Arrange
            var department1 = new Department { Name = "Zebra Department", Description = "Last alphabetically" };
            var department2 = new Department { Name = "Alpha Department", Description = "First alphabetically" };
            var department3 = new Department { Name = "Beta Department", Description = "Second alphabetically" };

            _context.Departments.AddRange(department1, department2, department3);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams
            {
                SortBy = "Name",
                SortDescending = false
            };

            // Act
            var result = await _repository.GetAllDepartmentsFilteredAsync(null, parameters);

            // Assert
            result.Data.Should().HaveCount(3);
            result.Data[0].Name.Should().Be("Alpha Department");
            result.Data[1].Name.Should().Be("Beta Department");
            result.Data[2].Name.Should().Be("Zebra Department");
        }

        [Fact]
        public async Task GetAllDepartmentsFilteredAsync_ShouldIncludeDeletedWhenRequested()
        {
            // Arrange
            var activeDepartment = new Department { Name = "Active", Description = "Active department" };
            var deletedDepartment = new Department 
            { 
                Name = "Deleted", 
                Description = "Deleted department",
                DeletedAt = DateTime.UtcNow
            };

            _context.Departments.AddRange(activeDepartment, deletedDepartment);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams();

            // Act
            var result = await _repository.GetAllDepartmentsFilteredAsync(null, parameters, includeDeleted: true);

            // Assert
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.Data.Should().Contain(d => d.Name == "Active");
            result.Data.Should().Contain(d => d.Name == "Deleted");
        }

        [Fact]
        public async Task GetAllInactiveDepartmentsFilteredAsync_ShouldReturnOnlyDeletedDepartments()
        {
            // Arrange
            var activeDepartment = new Department { Name = "Active", Description = "Active department" };
            var deletedDepartment1 = new Department 
            { 
                Name = "Deleted 1", 
                Description = "First deleted department",
                DeletedAt = DateTime.UtcNow.AddDays(-1)
            };
            var deletedDepartment2 = new Department 
            { 
                Name = "Deleted 2", 
                Description = "Second deleted department",
                DeletedAt = DateTime.UtcNow
            };

            _context.Departments.AddRange(activeDepartment, deletedDepartment1, deletedDepartment2);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams();

            // Act
            var result = await _repository.GetAllInactiveDepartmentsFilteredAsync(null, parameters);

            // Assert
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.Data.Should().Contain(d => d.Name == "Deleted 1");
            result.Data.Should().Contain(d => d.Name == "Deleted 2");
            result.Data.Should().NotContain(d => d.Name == "Active");
            result.Data.All(d => d.DeletedAt.HasValue).Should().BeTrue();
        }

        [Fact]
        public async Task GetAllInactiveDepartmentsFilteredAsync_ShouldFilterByName()
        {
            // Arrange
            var deletedDepartment1 = new Department 
            { 
                Name = "Closed IT Department", 
                Description = "Former IT department",
                DeletedAt = DateTime.UtcNow.AddDays(-1)
            };
            var deletedDepartment2 = new Department 
            { 
                Name = "Closed HR Department", 
                Description = "Former HR department",
                DeletedAt = DateTime.UtcNow
            };

            _context.Departments.AddRange(deletedDepartment1, deletedDepartment2);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams();

            // Act
            var result = await _repository.GetAllInactiveDepartmentsFilteredAsync("IT", parameters);

            // Assert
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Data[0].Name.Should().Be("Closed IT Department");
        }

        [Fact]
        public async Task GetAllInactiveDepartmentsFilteredAsync_ShouldApplyPagination()
        {
            // Arrange
            var deletedDepartments = new List<Department>();
            for (int i = 1; i <= 5; i++)
            {
                deletedDepartments.Add(new Department
                {
                    Name = $"Deleted Department {i}",
                    Description = $"Description for Deleted Department {i}",
                    DeletedAt = DateTime.UtcNow.AddDays(-i)
                });
            }

            _context.Departments.AddRange(deletedDepartments);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams
            {
                Page = 2,
                PageSize = 2
            };

            // Act
            var result = await _repository.GetAllInactiveDepartmentsFilteredAsync(null, parameters);

            // Assert
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(5);
        }

        [Fact]
        public async Task GetAllInactiveDepartmentsFilteredAsync_ShouldApplySorting()
        {
            // Arrange
            var deletedDepartment1 = new Department 
            { 
                Name = "Zebra Deleted Department", 
                Description = "Last alphabetically",
                DeletedAt = DateTime.UtcNow.AddDays(-1)
            };
            var deletedDepartment2 = new Department 
            { 
                Name = "Alpha Deleted Department", 
                Description = "First alphabetically",
                DeletedAt = DateTime.UtcNow.AddDays(-2)
            };

            _context.Departments.AddRange(deletedDepartment1, deletedDepartment2);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams
            {
                SortBy = "Name",
                SortDescending = false
            };

            // Act
            var result = await _repository.GetAllInactiveDepartmentsFilteredAsync(null, parameters);

            // Assert
            result.Data.Should().HaveCount(2);
            result.Data[0].Name.Should().Be("Alpha Deleted Department");
            result.Data[1].Name.Should().Be("Zebra Deleted Department");
        }

        [Fact]
        public async Task GetAllDepartmentsFilteredAsync_ShouldReturnWithoutPaginationWhenParametersNull()
        {
            // Arrange
            var department1 = new Department { Name = "IT", Description = "Information Technology" };
            var department2 = new Department { Name = "HR", Description = "Human Resources" };

            _context.Departments.AddRange(department1, department2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllDepartmentsFilteredAsync(null, null!);

            // Assert
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetAllDepartmentsFilteredAsync_ShouldReturnWithoutPaginationWhenInvalidPageParameters()
        {
            // Arrange
            var department1 = new Department { Name = "IT", Description = "Information Technology" };
            var department2 = new Department { Name = "HR", Description = "Human Resources" };

            _context.Departments.AddRange(department1, department2);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams
            {
                Page = 0, // Invalid
                PageSize = -1 // Invalid
            };

            // Act
            var result = await _repository.GetAllDepartmentsFilteredAsync(null, parameters);

            // Assert
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetAllInactiveDepartmentsFilteredAsync_ShouldReturnWithoutPaginationWhenParametersNull()
        {
            // Arrange
            var deletedDepartment = new Department 
            { 
                Name = "Deleted Department", 
                Description = "This was deleted",
                DeletedAt = DateTime.UtcNow
            };

            _context.Departments.Add(deletedDepartment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllInactiveDepartmentsFilteredAsync(null, null!);

            // Assert
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
