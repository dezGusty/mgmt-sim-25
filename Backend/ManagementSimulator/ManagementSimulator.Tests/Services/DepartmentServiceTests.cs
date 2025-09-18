using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementSimulator.Core.Dtos.Requests.Departments;
using ManagementSimulator.Core.Enums;
using ManagementSimulator.Core.Services;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Infrastructure.Exceptions;
using NSubstitute;
using Xunit;

namespace ManagementSimulator.Tests.Services
{
    public class DepartmentServiceTests
    {
        private readonly IDeparmentRepository _departmentRepo = Substitute.For<IDeparmentRepository>();

        private DepartmentService CreateService() => new DepartmentService(_departmentRepo);

        [Fact]
        public async Task GetAllDepartmentsAsync_Should_Return_All_Departments()
        {
            var service = CreateService();
            var departments = new List<Department>
            {
                new Department { Id = 1, Name = "IT", Description = "Information Technology" },
                new Department { Id = 2, Name = "HR", Description = "Human Resources" },
                new Department { Id = 3, Name = "Finance", Description = "Financial Department" }
            };
            _departmentRepo.GetAllAsync().Returns(departments);

            var result = await service.GetAllDepartmentsAsync();

            result.Should().HaveCount(3);
            result[0].Name.Should().Be("IT");
            result[1].Name.Should().Be("HR");
            result[2].Name.Should().Be("Finance");
        }

        [Fact]
        public async Task GetDepartmentByIdAsync_Should_Return_Department_When_Exists()
        {
            var service = CreateService();
            var department = new Department
            {
                Id = 1,
                Name = "IT",
                Description = "Information Technology Department"
            };
            _departmentRepo.GetFirstOrDefaultAsync(1).Returns(department);

            var result = await service.GetDepartmentByIdAsync(1);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Name.Should().Be("IT");
            result.Description.Should().Be("Information Technology Department");
        }

        [Fact]
        public async Task GetDepartmentByIdAsync_Should_Throw_When_Department_Not_Found()
        {
            var service = CreateService();
            _departmentRepo.GetFirstOrDefaultAsync(999).Returns((Department?)null);

            await Assert.ThrowsAsync<EntryNotFoundException>(() => service.GetDepartmentByIdAsync(999));
        }

        [Fact]
        public async Task AddDepartmentAsync_Should_Create_Department_Successfully()
        {
            var service = CreateService();
            var createDto = new CreateDepartmentRequestDto
            {
                Name = "Marketing",
                Description = "Marketing Department"
            };
            _departmentRepo.GetDepartmentByNameAsync("Marketing", true, false).Returns((Department?)null);
            _departmentRepo.AddAsync(Arg.Any<Department>()).Returns(ci => ci.Arg<Department>());

            var result = await service.AddDepartmentAsync(createDto);

            result.Should().NotBeNull();
            result.Name.Should().Be("Marketing");
            result.Description.Should().Be("Marketing Department");
            await _departmentRepo.Received(1).AddAsync(Arg.Is<Department>(d =>
                d.Name == "Marketing" && d.Description == "Marketing Department"));
        }

        [Fact]
        public async Task AddDepartmentAsync_Should_Handle_Null_Description()
        {
            var service = CreateService();
            var createDto = new CreateDepartmentRequestDto
            {
                Name = "Sales",
                Description = null
            };
            _departmentRepo.GetDepartmentByNameAsync("Sales", true, false).Returns((Department?)null);
            _departmentRepo.AddAsync(Arg.Any<Department>()).Returns(ci => ci.Arg<Department>());

            var result = await service.AddDepartmentAsync(createDto);

            result.Should().NotBeNull();
            result.Name.Should().Be("Sales");
            result.Description.Should().Be(string.Empty);
        }

        [Fact]
        public async Task AddDepartmentAsync_Should_Throw_When_Name_Already_Exists()
        {
            var service = CreateService();
            var existingDepartment = new Department { Name = "IT" };
            var createDto = new CreateDepartmentRequestDto
            {
                Name = "IT",
                Description = "Information Technology"
            };
            _departmentRepo.GetDepartmentByNameAsync("IT", true, false).Returns(existingDepartment);

            await Assert.ThrowsAsync<UniqueConstraintViolationException>(() => service.AddDepartmentAsync(createDto));
        }

        [Fact]
        public async Task UpdateDepartmentAsync_Should_Update_Department_Successfully()
        {
            var service = CreateService();
            var existingDepartment = new Department
            {
                Id = 1,
                Name = "IT",
                Description = "Old Description",
                ModifiedAt = null
            };
            var updateDto = new UpdateDepartmentRequestDto
            {
                Name = "Information Technology",
                Description = "Updated IT Department"
            };
            _departmentRepo.GetFirstOrDefaultAsync(1).Returns(Task.FromResult<Department?>(existingDepartment));
            _departmentRepo.UpdateAsync(Arg.Any<Department>()).Returns(callInfo => Task.FromResult<Department?>(callInfo.Arg<Department>()));
            _departmentRepo.When(x => x.UpdateAsync(Arg.Any<Department>())).Do(async (callInfo) =>
            {
                await _departmentRepo.SaveChangesAsync();
            });

            var result = await service.UpdateDepartmentAsync(1, updateDto);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Name.Should().Be("Information Technology");
            result.Description.Should().Be("Updated IT Department");
            await _departmentRepo.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task UpdateDepartmentAsync_Should_Throw_When_Department_Not_Found()
        {
            var service = CreateService();
            var updateDto = new UpdateDepartmentRequestDto
            {
                Name = "Updated Name"
            };
            _departmentRepo.GetFirstOrDefaultAsync(999).Returns((Department?)null);

            await Assert.ThrowsAsync<EntryNotFoundException>(() => service.UpdateDepartmentAsync(999, updateDto));
        }

        [Fact]
        public async Task DeleteDepartmentAsync_Should_Delete_Department_Successfully()
        {
            var service = CreateService();
            var department = new Department { Id = 1, Name = "IT" };
            _departmentRepo.GetFirstOrDefaultAsync(1).Returns(department);
            _departmentRepo.DeleteAsync(1).Returns(true);

            var result = await service.DeleteDepartmentAsync(1);

            result.Should().BeTrue();
            await _departmentRepo.Received(1).DeleteAsync(1);
        }

        [Fact]
        public async Task DeleteDepartmentAsync_Should_Throw_When_Department_Not_Found()
        {
            var service = CreateService();
            _departmentRepo.GetFirstOrDefaultAsync(999).Returns((Department?)null);

            await Assert.ThrowsAsync<EntryNotFoundException>(() => service.DeleteDepartmentAsync(999));
        }

        [Fact]
        public async Task RestoreDepartmentAsync_Should_Restore_Deleted_Department()
        {
            var service = CreateService();
            var deletedDepartment = new Department
            {
                Id = 1,
                Name = "IT",
                DeletedAt = DateTime.UtcNow.AddDays(-1)
            };
            _departmentRepo.GetDepartmentByIdAsync(1, true, true).Returns(deletedDepartment);

            var result = await service.RestoreDepartmentAsync(1);

            result.Should().BeTrue();
            deletedDepartment.DeletedAt.Should().BeNull();
            await _departmentRepo.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task RestoreDepartmentAsync_Should_Throw_When_Department_Not_Found()
        {
            var service = CreateService();
            _departmentRepo.GetDepartmentByIdAsync(999, true, true).Returns((Department?)null);

            await Assert.ThrowsAsync<EntryNotFoundException>(() => service.RestoreDepartmentAsync(999));
        }
    }
}