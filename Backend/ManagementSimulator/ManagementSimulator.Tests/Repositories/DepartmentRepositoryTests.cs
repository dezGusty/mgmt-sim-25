using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories;
using ManagementSimulator.Database.Dtos.QueryParams;
using ManagementSimulator.Database.Dtos.Department;
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
                Name = "IT Department",
                Description = "Information Technology Department"
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetDepartmentByNameAsync("IT Department");

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("IT Department");
            result.Description.Should().Be("Information Technology Department");
        }

        [Fact]
        public async Task GetDepartmentByNameAsync_ShouldReturnNullWhenNotFound()
        {
            // Arrange
            var department = new Department
            {
                Name = "IT Department",
                Description = "Information Technology Department"
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetDepartmentByNameAsync("Non Existent Department");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetDepartmentByNameAsync_ShouldExcludeDeletedDepartmentsByDefault()
        {
            // Arrange
            var department = new Department
            {
                Name = "IT Department",
                Description = "Information Technology Department",
                DeletedAt = DateTime.UtcNow
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetDepartmentByNameAsync("IT Department");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetDepartmentByNameAsync_ShouldIncludeDeletedDepartmentsWhenRequested()
        {
            // Arrange
            var department = new Department
            {
                Name = "IT Department",
                Description = "Information Technology Department",
                DeletedAt = DateTime.UtcNow
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetDepartmentByNameAsync("IT Department", includeDeleted: true);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("IT Department");
            result.DeletedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDepartmentByIdAsync_ShouldReturnDepartmentWhenFound()
        {
            // Arrange
            var department = new Department
            {
                Name = "IT Department",
                Description = "Information Technology Department"
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetDepartmentByIdAsync(department.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(department.Id);
            result.Name.Should().Be("IT Department");
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
        public async Task GetAllDepartmentsFilteredAsync_ShouldReturnAllActiveDepartments()
        {
            // Arrange
            var department1 = new Department
            {
                Name = "IT Department",
                Description = "Information Technology"
            };
            var department2 = new Department
            {
                Name = "HR Department",
                Description = "Human Resources"
            };
            var deletedDepartment = new Department
            {
                Name = "Old Department",
                Description = "Deleted Department",
                DeletedAt = DateTime.UtcNow
            };

            _context.Departments.AddRange(department1, department2, deletedDepartment);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams();

            // Act
            var result = await _repository.GetAllDepartmentsFilteredAsync(null, parameters);

            // Assert
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.Data.Should().Contain(d => d.Name == "IT Department");
            result.Data.Should().Contain(d => d.Name == "HR Department");
            result.Data.Should().NotContain(d => d.Name == "Old Department");
        }

        [Fact]
        public async Task GetAllDepartmentsFilteredAsync_ShouldFilterByName()
        {
            // Arrange
            var department1 = new Department
            {
                Name = "IT Department",
                Description = "Information Technology"
            };
            var department2 = new Department
            {
                Name = "HR Department",
                Description = "Human Resources"
            };

            _context.Departments.AddRange(department1, department2);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams();

            // Act
            var result = await _repository.GetAllDepartmentsFilteredAsync("IT", parameters);

            // Assert
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Data[0].Name.Should().Be("IT Department");
        }

        [Fact]
        public async Task GetAllDepartmentsFilteredAsync_ShouldIncludeEmployeeCount()
        {
            // Arrange
            var department = new Department
            {
                Name = "IT Department",
                Description = "Information Technology"
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            // Add users to department
            var user1 = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                DepartmentId = department.Id,
                JobTitleId = 1 // Assuming JobTitle with ID 1 exists
            };
            var user2 = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                PasswordHash = "hash",
                DepartmentId = department.Id,
                JobTitleId = 1
            };

            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams();

            // Act
            var result = await _repository.GetAllDepartmentsFilteredAsync(null, parameters);

            // Assert
            result.Data.Should().HaveCount(1);
            result.Data[0].EmployeeCount.Should().Be(2);
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
                    Description = $"Description {i}"
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
            var department1 = new Department
            {
                Name = "Z Department",
                Description = "Last Department"
            };
            var department2 = new Department
            {
                Name = "A Department",
                Description = "First Department"
            };

            _context.Departments.AddRange(department1, department2);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams
            {
                SortBy = "Name",
                SortDescending = false
            };

            // Act
            var result = await _repository.GetAllDepartmentsFilteredAsync(null, parameters);

            // Assert
            result.Data.Should().HaveCount(2);
            result.Data[0].Name.Should().Be("A Department");
            result.Data[1].Name.Should().Be("Z Department");
        }

        [Fact]
        public async Task GetAllDepartmentsFilteredAsync_ShouldApplyDescendingSorting()
        {
            // Arrange
            var department1 = new Department
            {
                Name = "A Department",
                Description = "First Department"
            };
            var department2 = new Department
            {
                Name = "Z Department",
                Description = "Last Department"
            };

            _context.Departments.AddRange(department1, department2);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams
            {
                SortBy = "Name",
                SortDescending = true
            };

            // Act
            var result = await _repository.GetAllDepartmentsFilteredAsync(null, parameters);

            // Assert
            result.Data.Should().HaveCount(2);
            result.Data[0].Name.Should().Be("Z Department");
            result.Data[1].Name.Should().Be("A Department");
        }

        [Fact]
        public async Task GetAllInactiveDepartmentsFilteredAsync_ShouldReturnOnlyDeletedDepartments()
        {
            // Arrange
            var activeDepartment = new Department
            {
                Name = "Active Department",
                Description = "Active Department"
            };
            var deletedDepartment = new Department
            {
                Name = "Deleted Department",
                Description = "Deleted Department",
                DeletedAt = DateTime.UtcNow
            };

            _context.Departments.AddRange(activeDepartment, deletedDepartment);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams();

            // Act
            var result = await _repository.GetAllInactiveDepartmentsFilteredAsync(null, parameters);

            // Assert
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Data[0].Name.Should().Be("Deleted Department");
            result.Data[0].DeletedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllDepartmentsAsync_ShouldReturnDepartmentsByIds()
        {
            // Arrange
            var department1 = new Department
            {
                Name = "Department 1",
                Description = "Description 1"
            };
            var department2 = new Department
            {
                Name = "Department 2",
                Description = "Description 2"
            };
            var department3 = new Department
            {
                Name = "Department 3",
                Description = "Description 3"
            };

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

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
