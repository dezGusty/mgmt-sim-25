using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementSimulator.Core.Dtos.Requests.Users;
using ManagementSimulator.Core.Dtos.Responses.PagedResponse;
using ManagementSimulator.Core.Dtos.Responses.User;
using ManagementSimulator.Core.Dtos.Responses.Users;
using ManagementSimulator.Core.Services;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Enums;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Infrastructure.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Xunit;

namespace ManagementSimulator.Tests.Services
{
    public class UserServiceTests
    {
        private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
        private readonly IJobTitleRepository _jobTitleRepository = Substitute.For<IJobTitleRepository>();
        private readonly IEmployeeRoleRepository _employeeRoleRepository = Substitute.For<IEmployeeRoleRepository>();
        private readonly IDeparmentRepository _departmentRepository = Substitute.For<IDeparmentRepository>();
        private readonly IEmailService _emailService = Substitute.For<IEmailService>();
        private readonly IEmployeeManagerService _employeeManagerService = Substitute.For<IEmployeeManagerService>();
        private readonly IMemoryCache _cache = Substitute.For<IMemoryCache>();
        private readonly ILeaveRequestTypeRepository _leaveRequestTypeRepository = Substitute.For<ILeaveRequestTypeRepository>();
        private readonly ILeaveRequestRepository _leaveRequestRepository = Substitute.For<ILeaveRequestRepository>();
        private readonly IAvailabilityService _availabilityService = Substitute.For<IAvailabilityService>();
        private readonly IWeekendService _weekendService = Substitute.For<IWeekendService>();

        private UserService CreateService()
        {
            var dbContext = DbContextFactory.CreateInMemoryContext();
            return new UserService(
                _userRepository,
                _jobTitleRepository,
                _employeeRoleRepository,
                _departmentRepository,
                _emailService,
                _employeeManagerService,
                _cache,
                _leaveRequestTypeRepository,
                _leaveRequestRepository,
                dbContext,
                _availabilityService,
                _weekendService);
        }

        [Fact]
        public async Task GetAllUsersAsync_Should_Return_All_Users_With_References()
        {
            // Arrange
            var service = CreateService();
            var users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Email = "user1@test.com",
                    FirstName = "John",
                    LastName = "Doe",
                    JobTitleId = 1,
                    DepartmentId = 1,
                    Vacation = 21,
                    Title = new JobTitle { Name = "Developer" },
                    Department = new Department { Name = "IT" },
                    Roles = new List<EmployeeRoleUser>
                    {
                        new EmployeeRoleUser { Role = new EmployeeRole { Rolename = "Employee" } }
                    }
                },
                new User
                {
                    Id = 2,
                    Email = "user2@test.com",
                    FirstName = "Jane",
                    LastName = "Smith",
                    JobTitleId = 2,
                    DepartmentId = 2,
                    Vacation = 25,
                    DeletedAt = DateTime.UtcNow,
                    Title = new JobTitle { Name = "Manager" },
                    Department = new Department { Name = "HR" },
                    Roles = new List<EmployeeRoleUser>
                    {
                        new EmployeeRoleUser { Role = new EmployeeRole { Rolename = "Manager" } }
                    }
                }
            };

            _userRepository.GetAllUsersWithReferencesAsync(includeDeleted: true).Returns(users);

            // Act
            var result = await service.GetAllUsersAsync();

            // Assert
            result.Should().HaveCount(2);
            result[0].Id.Should().Be(1);
            result[0].Email.Should().Be("user1@test.com");
            result[0].FirstName.Should().Be("John");
            result[0].LastName.Should().Be("Doe");
            result[0].IsActive.Should().BeTrue();
            result[0].JobTitleName.Should().Be("Developer");
            result[0].DepartmentName.Should().Be("IT");
            result[0].Vacation.Should().Be(21);

            result[1].Id.Should().Be(2);
            result[1].IsActive.Should().BeFalse();
            result[1].Vacation.Should().Be(25);
        }

        [Fact]
        public async Task GetUserByIdAsync_Should_Return_User_When_Exists()
        {
            // Arrange
            var service = CreateService();
            var user = new User
            {
                Id = 1,
                Email = "user@test.com",
                FirstName = "John",
                LastName = "Doe",
                JobTitleId = 1,
                DepartmentId = 1,
                Vacation = 21,
                Title = new JobTitle { Name = "Developer" },
                Department = new Department { Name = "IT" },
                Roles = new List<EmployeeRoleUser>
                {
                    new EmployeeRoleUser { Role = new EmployeeRole { Rolename = "Employee" } }
                }
            };

            _userRepository.GetUserWithReferencesByIdAsync(1).Returns(user);

            // Act
            var result = await service.GetUserByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Email.Should().Be("user@test.com");
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
        }

        [Fact]
        public async Task GetUserByIdAsync_Should_Throw_When_User_Not_Found()
        {
            // Arrange
            var service = CreateService();
            _userRepository.GetUserWithReferencesByIdAsync(999).Returns((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntryNotFoundException>(() => service.GetUserByIdAsync(999));
        }

        [Fact]
        public async Task AddUserAsync_Should_Create_User_Successfully()
        {
            // Arrange
            var service = CreateService();
            var createDto = new CreateUserRequestDto
            {
                Email = "newuser@test.com",
                FirstName = "New",
                LastName = "User",
                JobTitleId = 1,
                DepartmentId = 1,
                EmployeeRolesId = new List<int> { 2 },
                DateOfEmployment = DateTime.Now,
                Vacation = 21,
                EmploymentType = EmploymentType.FullTime
            };

            var jobTitle = new JobTitle { Id = 1, Name = "Developer" };
            var createdUser = new User
            {
                Id = 1,
                Email = "newuser@test.com",
                FirstName = "New",
                LastName = "User",
                JobTitleId = 1,
                DepartmentId = 1,
                Title = jobTitle,
                Vacation = 21
            };

            _userRepository.GetUserByEmail("newuser@test.com", includeDeleted: true).Returns((User?)null);
            _jobTitleRepository.GetFirstOrDefaultAsync(1).Returns(jobTitle);
            _employeeRoleRepository.GetEmployeeRoleUserByNameAsync("Employee").Returns(1);
            _employeeRoleRepository.GetFirstOrDefaultAsync(2).Returns(new EmployeeRole { Id = 2, Rolename = "Manager" });
            _employeeRoleRepository.GetEmployeeRoleUserAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<bool>()).Returns((EmployeeRoleUser?)null);
            _userRepository.AddAsync(Arg.Any<User>()).Returns(ci =>
            {
                var user = ci.Arg<User>();
                user.Id = 1;
                return user;
            });

            // Act
            var result = await service.AddUserAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be("newuser@test.com");
            result.FirstName.Should().Be("New");
            result.LastName.Should().Be("User");

            await _userRepository.Received(1).AddAsync(Arg.Is<User>(u =>
                u.Email == "newuser@test.com" &&
                u.FirstName == "New" &&
                u.LastName == "User" &&
                u.MustChangePassword == true &&
                u.Vacation == 21));

            await _employeeRoleRepository.Received(2).AddEmployeeRoleUserAsync(Arg.Any<EmployeeRoleUser>());
            await _emailService.Received(1).SendWelcomeEmailWithPasswordAsync(
                "newuser@test.com",
                "New",
                Arg.Any<string>());
        }

        [Fact]
        public async Task AddUserAsync_Should_Throw_When_Email_Already_Exists()
        {
            // Arrange
            var service = CreateService();
            var createDto = new CreateUserRequestDto
            {
                Email = "existing@test.com",
                FirstName = "Test",
                LastName = "User"
            };

            var existingUser = new User { Email = "existing@test.com" };
            _userRepository.GetUserByEmail("existing@test.com", includeDeleted: true).Returns(existingUser);

            // Act & Assert
            await Assert.ThrowsAsync<UniqueConstraintViolationException>(() => service.AddUserAsync(createDto));
        }

        [Fact]
        public async Task AddUserAsync_Should_Throw_When_JobTitle_Not_Found()
        {
            // Arrange
            var service = CreateService();
            var createDto = new CreateUserRequestDto
            {
                Email = "newuser@test.com",
                JobTitleId = 999
            };

            _userRepository.GetUserByEmail("newuser@test.com", includeDeleted: true).Returns((User?)null);
            _jobTitleRepository.GetFirstOrDefaultAsync(999).Returns((JobTitle?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntryNotFoundException>(() => service.AddUserAsync(createDto));
        }

        [Fact]
        public async Task AddUserAsync_Should_Throw_MailNotSentException_When_Email_Fails()
        {
            // Arrange
            var service = CreateService();
            var createDto = new CreateUserRequestDto
            {
                Email = "newuser@test.com",
                FirstName = "New",
                LastName = "User",
                JobTitleId = 1,
                DepartmentId = 1,
                EmployeeRolesId = new List<int>(),
                DateOfEmployment = DateTime.Now
            };

            var jobTitle = new JobTitle { Id = 1, Name = "Developer" };

            _userRepository.GetUserByEmail("newuser@test.com", includeDeleted: true).Returns((User?)null);
            _jobTitleRepository.GetFirstOrDefaultAsync(1).Returns(jobTitle);
            _employeeRoleRepository.GetEmployeeRoleUserByNameAsync("Employee").Returns(1);
            _userRepository.AddAsync(Arg.Any<User>()).Returns(ci =>
            {
                var user = ci.Arg<User>();
                user.Id = 1;
                return user;
            });
            _emailService.SendWelcomeEmailWithPasswordAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromException(new Exception("Email failed")));

            // Act & Assert
            await Assert.ThrowsAsync<MailNotSentException>(() => service.AddUserAsync(createDto));
        }

        [Fact]
        public async Task UpdateUserAsync_Should_Update_User_Successfully()
        {
            // Arrange
            var service = CreateService();
            var existingUser = new User
            {
                Id = 1,
                Email = "old@test.com",
                FirstName = "Old",
                LastName = "Name",
                JobTitleId = 1,
                DepartmentId = 1,
                Vacation = 21,
                Title = new JobTitle { Id = 1, Name = "Developer" },
                Department = new Department { Id = 1, Name = "IT" },
                Roles = new List<EmployeeRoleUser>()
            };

            var updateDto = new UpdateUserRequestDto
            {
                Email = "new@test.com",
                FirstName = "New",
                LastName = "Name",
                JobTitleId = 2,
                Vacation = 25
            };

            var newJobTitle = new JobTitle { Id = 2, Name = "Senior Developer" };

            _userRepository.GetUserWithReferencesByIdAsync(1, tracking: true).Returns(existingUser);
            _userRepository.GetUserByEmail("new@test.com").Returns((User?)null);
            _jobTitleRepository.GetFirstOrDefaultAsync(2).Returns(newJobTitle);
            _employeeRoleRepository.GetEmployeeRoleUsersByUserIdAsync(existingUser.Id, tracking: false).Returns(new List<EmployeeRoleUser>());
            _userRepository.UpdateAsync(Arg.Any<User>()).Returns(Task.FromResult<User?>(existingUser));

            // Act
            var result = await service.UpdateUserAsync(1, updateDto);

            // Assert
            result.Should().NotBeNull();
            existingUser.Title.Should().Be(newJobTitle);
            existingUser.Vacation.Should().Be(25);

            await _userRepository.Received(1).UpdateAsync(existingUser);
        }

        [Fact]
        public async Task UpdateUserAsync_Should_Throw_When_User_Not_Found()
        {
            // Arrange
            var service = CreateService();
            var updateDto = new UpdateUserRequestDto { Email = "test@test.com" };

            _userRepository.GetUserWithReferencesByIdAsync(999, tracking: true).Returns((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntryNotFoundException>(() => service.UpdateUserAsync(999, updateDto));
        }

        [Fact]
        public async Task UpdateUserAsync_Should_Throw_When_Email_Already_Exists()
        {
            // Arrange
            var service = CreateService();
            var existingUser = new User
            {
                Id = 1,
                Email = "old@test.com"
            };

            var updateDto = new UpdateUserRequestDto
            {
                Email = "existing@test.com"
            };

            var userWithEmail = new User { Email = "existing@test.com" };

            _userRepository.GetUserWithReferencesByIdAsync(1, tracking: true).Returns(existingUser);
            _userRepository.GetUserByEmail("existing@test.com").Returns(userWithEmail);

            // Act & Assert
            await Assert.ThrowsAsync<UniqueConstraintViolationException>(() => service.UpdateUserAsync(1, updateDto));
        }

        [Fact]
        public async Task DeleteUserAsync_Should_Delete_User_Successfully()
        {
            // Arrange
            var service = CreateService();
            var user = new User { Id = 1, Email = "test@test.com" };

            _userRepository.GetFirstOrDefaultAsync(1).Returns(user);
            _userRepository.DeleteAsync(1).Returns(true);

            // Act
            var result = await service.DeleteUserAsync(1);

            // Assert
            result.Should().BeTrue();
            await _userRepository.Received(1).DeleteAsync(1);
            await _employeeManagerService.Received(1).SetSubordinatesToUnassignedAsync(1);
        }

        [Fact]
        public async Task DeleteUserAsync_Should_Throw_When_User_Not_Found()
        {
            // Arrange
            var service = CreateService();
            _userRepository.GetFirstOrDefaultAsync(999).Returns((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntryNotFoundException>(() => service.DeleteUserAsync(999));
        }

        [Fact]
        public async Task RestoreUserByIdAsync_Should_Restore_User_Successfully()
        {
            // Arrange
            var service = CreateService();
            var deletedUser = new User
            {
                Id = 1,
                Email = "test@test.com",
                DeletedAt = DateTime.UtcNow.AddDays(-1)
            };

            _userRepository.GetUserByIdAsync(1, includeDeleted: true, tracking: true).Returns(deletedUser);

            // Act
            await service.RestoreUserByIdAsync(1);

            // Assert
            deletedUser.DeletedAt.Should().BeNull();
            await _userRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task RestoreUserByIdAsync_Should_Throw_When_User_Not_Found()
        {
            // Arrange
            var service = CreateService();
            _userRepository.GetUserByIdAsync(999, includeDeleted: true, tracking: true).Returns((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntryNotFoundException>(() => service.RestoreUserByIdAsync(999));
        }

        [Fact]
        public async Task SendPasswordResetCodeAsync_Should_Return_True_When_User_Exists()
        {
            // Arrange
            var service = CreateService();
            var user = new User
            {
                Email = "test@test.com",
                FirstName = "Test"
            };

            _userRepository.GetUserByEmail("test@test.com").Returns(user);
            _emailService.SendPasswordResetCodeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.CompletedTask);

            // Act
            var result = await service.SendPasswordResetCodeAsync("test@test.com");

            // Assert
            result.Should().BeTrue();
            await _emailService.Received(1).SendPasswordResetCodeAsync("test@test.com", "Test", Arg.Any<string>());
            _cache.Received(1).Set(Arg.Is<string>(key => key.StartsWith("reset_code_")), "test@test.com", TimeSpan.FromMinutes(15));
        }

        [Fact]
        public async Task SendPasswordResetCodeAsync_Should_Return_False_When_User_Not_Found()
        {
            // Arrange
            var service = CreateService();
            _userRepository.GetUserByEmail("nonexistent@test.com").Returns((User?)null);

            // Act
            var result = await service.SendPasswordResetCodeAsync("nonexistent@test.com");

            // Assert
            result.Should().BeFalse();
            await _emailService.DidNotReceive().SendPasswordResetCodeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ResetPasswordWithCodeAsync_Should_Return_True_When_Code_Valid()
        {
            // Arrange
            var service = CreateService();
            var user = new User
            {
                Email = "test@test.com",
                MustChangePassword = true
            };

            string email = "test@test.com";
            _cache.TryGetValue("reset_code_ABC123", out Arg.Any<object?>()).Returns(x =>
            {
                x[1] = email;
                return true;
            });
            _userRepository.GetUserByEmail("test@test.com", tracking: true).Returns(user);
            _userRepository.UpdateAsync(Arg.Any<User>()).Returns(Task.FromResult<User?>(user));

            // Act
            var result = await service.ResetPasswordWithCodeAsync("ABC123", "newpassword");

            // Assert
            result.Should().BeTrue();
            user.MustChangePassword.Should().BeFalse();
            await _userRepository.Received(1).UpdateAsync(user);
            _cache.Received(1).Remove("reset_code_ABC123");
        }

        [Fact]
        public async Task ResetPasswordWithCodeAsync_Should_Return_False_When_Code_Invalid()
        {
            // Arrange
            var service = CreateService();
            _cache.TryGetValue("reset_code_INVALID", out Arg.Any<object?>()).Returns(false);

            // Act
            var result = await service.ResetPasswordWithCodeAsync("INVALID", "newpassword");

            // Assert
            result.Should().BeFalse();
            await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
        }

        [Fact]
        public async Task GetUserByEmailAsync_Should_Return_User_When_Exists()
        {
            // Arrange
            var service = CreateService();
            var user = new User { Email = "test@test.com" };
            _userRepository.GetUserByEmail("test@test.com").Returns(user);

            // Act
            var result = await service.GetUserByEmailAsync("test@test.com");

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be("test@test.com");
        }

        [Fact]
        public async Task GetUserByEmailAsync_Should_Return_Null_When_User_Not_Found()
        {
            // Arrange
            var service = CreateService();
            _userRepository.GetUserByEmail("nonexistent@test.com").Returns((User?)null);

            // Act
            var result = await service.GetUserByEmailAsync("nonexistent@test.com");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AdjustUserVacationAsync_Should_Adjust_Vacation_Days_Successfully()
        {
            // Arrange
            var service = CreateService();
            var user = new User
            {
                Id = 1,
                Vacation = 21
            };

            _userRepository.GetUserByIdAsync(1, tracking: true).Returns(user);
            _userRepository.UpdateAsync(Arg.Any<User>()).Returns(Task.FromResult<User?>(user));

            // Act
            var result = await service.AdjustUserVacationAsync(1, 5);

            // Assert
            result.Should().Be(26);
            user.Vacation.Should().Be(26);
            await _userRepository.Received(1).UpdateAsync(user);
        }

        [Fact]
        public async Task AdjustUserVacationAsync_Should_Set_Minimum_Zero_When_Negative()
        {
            // Arrange
            var service = CreateService();
            var user = new User
            {
                Id = 1,
                Vacation = 5
            };

            _userRepository.GetUserByIdAsync(1, tracking: true).Returns(user);
            _userRepository.UpdateAsync(Arg.Any<User>()).Returns(Task.FromResult<User?>(user));

            // Act
            var result = await service.AdjustUserVacationAsync(1, -10);

            // Assert
            result.Should().Be(0);
            user.Vacation.Should().Be(0);
            await _userRepository.Received(1).UpdateAsync(user);
        }

        [Fact]
        public async Task AdjustUserVacationAsync_Should_Throw_When_User_Not_Found()
        {
            // Arrange
            var service = CreateService();
            _userRepository.GetUserByIdAsync(999, tracking: true).Returns((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntryNotFoundException>(() => service.AdjustUserVacationAsync(999, 5));
        }

        [Fact]
        public async Task GetTotalAdminsCountAsync_Should_Return_Admin_Count()
        {
            // Arrange
            var service = CreateService();
            _userRepository.GetTotalAdminsCountAsync(includeDeleted: false).Returns(5);

            // Act
            var result = await service.GetTotalAdminsCountAsync();

            // Assert
            result.Should().Be(5);
        }

        [Fact]
        public async Task GetTotalManagersCountAsync_Should_Return_Manager_Count()
        {
            // Arrange
            var service = CreateService();
            _userRepository.GetTotalManagersCountAsync(includeDeleted: false).Returns(10);

            // Act
            var result = await service.GetTotalManagersCountAsync();

            // Assert
            result.Should().Be(10);
        }

        [Fact]
        public async Task GetTotalUnassignedUsersCountAsync_Should_Return_Unassigned_Count()
        {
            // Arrange
            var service = CreateService();
            _userRepository.GetTotalUnassignedUsersCountAsync(includeDeleted: false).Returns(3);

            // Act
            var result = await service.GetTotalUnassignedUsersCountAsync();

            // Assert
            result.Should().Be(3);
        }

        [Fact]
        public async Task AddUserAsync_Should_Set_Default_Vacation_When_Not_Provided()
        {
            // Arrange
            var service = CreateService();
            var createDto = new CreateUserRequestDto
            {
                Email = "newuser@test.com",
                FirstName = "New",
                LastName = "User",
                JobTitleId = 1,
                DepartmentId = 1,
                EmployeeRolesId = new List<int>(),
                DateOfEmployment = DateTime.Now,
                Vacation = null // Not provided
            };

            var jobTitle = new JobTitle { Id = 1, Name = "Developer" };

            _userRepository.GetUserByEmail("newuser@test.com", includeDeleted: true).Returns((User?)null);
            _jobTitleRepository.GetFirstOrDefaultAsync(1).Returns(jobTitle);
            _employeeRoleRepository.GetEmployeeRoleUserByNameAsync("Employee").Returns(1);
            _userRepository.AddAsync(Arg.Any<User>()).Returns(ci =>
            {
                var user = ci.Arg<User>();
                user.Id = 1;
                return user;
            });

            // Act
            var result = await service.AddUserAsync(createDto);

            // Assert
            await _userRepository.Received(1).AddAsync(Arg.Is<User>(u => u.Vacation == 21));
        }

        [Fact]
        public async Task UpdateUserAsync_Should_Not_Change_Email_When_Same_As_Existing()
        {
            // Arrange
            var service = CreateService();
            var existingUser = new User
            {
                Id = 1,
                Email = "existing@test.com",
                FirstName = "Test",
                LastName = "User",
                JobTitleId = 1,
                DepartmentId = 1,
                Vacation = 21,
                Title = new JobTitle { Id = 1, Name = "Developer" },
                Department = new Department { Id = 1, Name = "IT" },
                Roles = new List<EmployeeRoleUser>()
            };

            var updateDto = new UpdateUserRequestDto
            {
                Email = "existing@test.com", // Same email
                FirstName = "Updated"
            };

            _userRepository.GetUserWithReferencesByIdAsync(1, tracking: true).Returns(existingUser);
            _employeeRoleRepository.GetEmployeeRoleUsersByUserIdAsync(existingUser.Id, tracking: false).Returns(new List<EmployeeRoleUser>());
            _userRepository.UpdateAsync(Arg.Any<User>()).Returns(Task.FromResult<User?>(existingUser));

            // Act
            var result = await service.UpdateUserAsync(1, updateDto);

            // Assert
            result.Should().NotBeNull();
            await _userRepository.DidNotReceive().GetUserByEmail("existing@test.com");
        }

        [Fact]
        public async Task UpdateUserAsync_Should_Throw_When_JobTitle_Not_Found()
        {
            // Arrange
            var service = CreateService();
            var existingUser = new User
            {
                Id = 1,
                Email = "test@test.com",
                JobTitleId = 1,
                Roles = new List<EmployeeRoleUser>()
            };

            var updateDto = new UpdateUserRequestDto
            {
                JobTitleId = 999
            };

            _userRepository.GetUserWithReferencesByIdAsync(1, tracking: true).Returns(existingUser);
            _jobTitleRepository.GetFirstOrDefaultAsync(999).Returns((JobTitle?)null);

            // Act & Assert
            await Assert.ThrowsAsync<EntryNotFoundException>(() => service.UpdateUserAsync(1, updateDto));
        }

        [Fact]
        public async Task SendPasswordResetCodeAsync_Should_Remove_Cache_When_Email_Fails()
        {
            // Arrange
            var service = CreateService();
            var user = new User
            {
                Email = "test@test.com",
                FirstName = "Test"
            };

            _userRepository.GetUserByEmail("test@test.com").Returns(user);
            _emailService.SendPasswordResetCodeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromException(new Exception("Email failed")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => service.SendPasswordResetCodeAsync("test@test.com"));
            _cache.Received(1).Remove(Arg.Is<string>(key => key.StartsWith("reset_code_")));
        }

        [Fact]
        public async Task DeleteUserAsync_Should_Handle_EmployeeManager_Exception_Gracefully()
        {
            // Arrange
            var service = CreateService();
            var user = new User { Id = 1, Email = "test@test.com" };

            _userRepository.GetFirstOrDefaultAsync(1).Returns(user);
            _userRepository.DeleteAsync(1).Returns(true);
            _employeeManagerService.SetSubordinatesToUnassignedAsync(1).Returns(Task.FromException(new EntryNotFoundException("Manager", 1)));

            // Act
            var result = await service.DeleteUserAsync(1);

            // Assert
            result.Should().BeTrue();
            await _userRepository.Received(1).DeleteAsync(1);
        }
    }
}
