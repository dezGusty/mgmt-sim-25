using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories;
using ManagementSimulator.Database.Dtos.QueryParams;
using ManagementSimulator.Database.Enums;
using ManagementSimulator.Tests;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace ManagementSimulator.Tests.Repositories
{
    public class LeaveRequestRepositoryTests : IDisposable
    {
        private readonly MGMTSimulatorDbContext _context;
        private readonly LeaveRequestRepository _repository;

        public LeaveRequestRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<MGMTSimulatorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MGMTSimulatorDbContext(options);
            _repository = new LeaveRequestRepository(_context, new TestAuditService());
        }

        [Fact]
        public async Task GetAllLeaveRequestsWithRelationshipsFilteredAsync_ShouldReturnFilteredResults()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };
            var leaveType = new LeaveRequestType { Title = "Vacation", MaxDays = 25, IsPaid = true };

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
            _context.LeaveRequestTypes.Add(leaveType);
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var leaveRequest1 = new LeaveRequest
            {
                UserId = user1.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 5),
                Reason = "Family vacation",
                RequestStatus = RequestStatus.Approved
            };
            var leaveRequest2 = new LeaveRequest
            {
                UserId = user2.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 7, 1),
                EndDate = new DateTime(2024, 7, 3),
                Reason = "Personal time",
                RequestStatus = RequestStatus.Pending
            };

            _context.LeaveRequests.AddRange(leaveRequest1, leaveRequest2);
            await _context.SaveChangesAsync();

            var employeeIds = new List<int> { user1.Id, user2.Id };
            var parameters = new QueryParams();

            // Act
            var result = await _repository.GetAllLeaveRequestsWithRelationshipsFilteredAsync(
                employeeIds, null, null, parameters);

            // Assert
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.Data.Should().Contain(lr => lr.UserId == user1.Id);
            result.Data.Should().Contain(lr => lr.UserId == user2.Id);
            result.Data.All(lr => lr.User != null).Should().BeTrue();
            result.Data.All(lr => lr.LeaveRequestType != null).Should().BeTrue();
        }

        [Fact]
        public async Task GetAllLeaveRequestsWithRelationshipsFilteredAsync_ShouldFilterByLastName()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };
            var leaveType = new LeaveRequestType { Title = "Vacation", MaxDays = 25, IsPaid = true };

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
            _context.LeaveRequestTypes.Add(leaveType);
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var leaveRequest1 = new LeaveRequest
            {
                UserId = user1.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 5),
                RequestStatus = RequestStatus.Approved
            };
            var leaveRequest2 = new LeaveRequest
            {
                UserId = user2.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 7, 1),
                EndDate = new DateTime(2024, 7, 3),
                RequestStatus = RequestStatus.Pending
            };

            _context.LeaveRequests.AddRange(leaveRequest1, leaveRequest2);
            await _context.SaveChangesAsync();

            var employeeIds = new List<int> { user1.Id, user2.Id };
            var parameters = new QueryParams();

            // Act
            var result = await _repository.GetAllLeaveRequestsWithRelationshipsFilteredAsync(
                employeeIds, "Doe", null, parameters);

            // Assert
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Data[0].User.LastName.Should().Be("Doe");
        }

        [Fact]
        public async Task GetAllLeaveRequestsWithRelationshipsFilteredAsync_ShouldFilterByEmail()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };
            var leaveType = new LeaveRequestType { Title = "Vacation", MaxDays = 25, IsPaid = true };

            var user1 = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Departments.Add(department);
            _context.JobTitles.Add(jobTitle);
            _context.LeaveRequestTypes.Add(leaveType);
            _context.Users.Add(user1);
            await _context.SaveChangesAsync();

            var leaveRequest = new LeaveRequest
            {
                UserId = user1.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 5),
                RequestStatus = RequestStatus.Approved
            };

            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();

            var employeeIds = new List<int> { user1.Id };
            var parameters = new QueryParams();

            // Act
            var result = await _repository.GetAllLeaveRequestsWithRelationshipsFilteredAsync(
                employeeIds, null, "john.doe", parameters);

            // Assert
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Data[0].User.Email.Should().Contain("john.doe");
        }

        [Fact]
        public async Task GetAllLeaveRequestsWithRelationshipsFilteredAsync_ShouldExcludeDeletedByDefault()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };
            var leaveType = new LeaveRequestType { Title = "Vacation", MaxDays = 25, IsPaid = true };

            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Departments.Add(department);
            _context.JobTitles.Add(jobTitle);
            _context.LeaveRequestTypes.Add(leaveType);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var activeRequest = new LeaveRequest
            {
                UserId = user.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 5),
                RequestStatus = RequestStatus.Approved
            };
            var deletedRequest = new LeaveRequest
            {
                UserId = user.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 7, 1),
                EndDate = new DateTime(2024, 7, 3),
                RequestStatus = RequestStatus.Pending,
                DeletedAt = DateTime.UtcNow
            };

            _context.LeaveRequests.AddRange(activeRequest, deletedRequest);
            await _context.SaveChangesAsync();

            var employeeIds = new List<int> { user.Id };
            var parameters = new QueryParams();

            // Act
            var result = await _repository.GetAllLeaveRequestsWithRelationshipsFilteredAsync(
                employeeIds, null, null, parameters);

            // Assert
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Data[0].DeletedAt.Should().BeNull();
        }

        [Fact]
        public async Task GetAllLeaveRequestsWithRelationshipsFilteredAsync_ShouldApplyPagination()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };
            var leaveType = new LeaveRequestType { Title = "Vacation", MaxDays = 25, IsPaid = true };

            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Departments.Add(department);
            _context.JobTitles.Add(jobTitle);
            _context.LeaveRequestTypes.Add(leaveType);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var requests = new List<LeaveRequest>();
            for (int i = 1; i <= 5; i++)
            {
                requests.Add(new LeaveRequest
                {
                    UserId = user.Id,
                    LeaveRequestTypeId = leaveType.Id,
                    StartDate = new DateTime(2024, i, 1),
                    EndDate = new DateTime(2024, i, 2),
                    RequestStatus = RequestStatus.Pending
                });
            }

            _context.LeaveRequests.AddRange(requests);
            await _context.SaveChangesAsync();

            var employeeIds = new List<int> { user.Id };
            var parameters = new QueryParams { Page = 2, PageSize = 2 };

            // Act
            var result = await _repository.GetAllLeaveRequestsWithRelationshipsFilteredAsync(
                employeeIds, null, null, parameters);

            // Assert
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(5);
        }

        [Fact]
        public async Task GetOverlappingRequestsAsync_ShouldReturnOverlappingRequests()
        {
            // Arrange
            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };
            var leaveType = new LeaveRequestType { Title = "Vacation", MaxDays = 25, IsPaid = true };

            _context.Users.Add(user);
            _context.LeaveRequestTypes.Add(leaveType);
            await _context.SaveChangesAsync();

            var existingRequest = new LeaveRequest
            {
                UserId = user.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 10),
                RequestStatus = RequestStatus.Approved
            };
            var nonOverlappingRequest = new LeaveRequest
            {
                UserId = user.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 7, 1),
                EndDate = new DateTime(2024, 7, 5),
                RequestStatus = RequestStatus.Approved
            };

            _context.LeaveRequests.AddRange(existingRequest, nonOverlappingRequest);
            await _context.SaveChangesAsync();

            var newRequestStart = new DateTime(2024, 6, 5);
            var newRequestEnd = new DateTime(2024, 6, 15);

            // Act
            var result = await _repository.GetOverlappingRequestsAsync(user.Id, newRequestStart, newRequestEnd);

            // Assert
            result.Should().HaveCount(1);
            result[0].Id.Should().Be(existingRequest.Id);
        }

        [Fact]
        public async Task GetOverlappingRequestsAsync_ShouldReturnEmptyWhenNoOverlap()
        {
            // Arrange
            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };
            var leaveType = new LeaveRequestType { Title = "Vacation", MaxDays = 25, IsPaid = true };

            _context.Users.Add(user);
            _context.LeaveRequestTypes.Add(leaveType);
            await _context.SaveChangesAsync();

            var existingRequest = new LeaveRequest
            {
                UserId = user.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 10),
                RequestStatus = RequestStatus.Approved
            };

            _context.LeaveRequests.Add(existingRequest);
            await _context.SaveChangesAsync();

            var newRequestStart = new DateTime(2024, 7, 1);
            var newRequestEnd = new DateTime(2024, 7, 5);

            // Act
            var result = await _repository.GetOverlappingRequestsAsync(user.Id, newRequestStart, newRequestEnd);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetLeaveRequestsByUserAndTypeAsync_ShouldReturnRequestsForUserAndType()
        {
            // Arrange
            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };
            var vacationType = new LeaveRequestType { Title = "Vacation", MaxDays = 25, IsPaid = true };
            var sickType = new LeaveRequestType { Title = "Sick Leave", MaxDays = 10, IsPaid = true };

            _context.Users.Add(user);
            _context.LeaveRequestTypes.AddRange(vacationType, sickType);
            await _context.SaveChangesAsync();

            var vacationRequest = new LeaveRequest
            {
                UserId = user.Id,
                LeaveRequestTypeId = vacationType.Id,
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 5),
                RequestStatus = RequestStatus.Approved
            };
            var sickRequest = new LeaveRequest
            {
                UserId = user.Id,
                LeaveRequestTypeId = sickType.Id,
                StartDate = new DateTime(2024, 7, 1),
                EndDate = new DateTime(2024, 7, 2),
                RequestStatus = RequestStatus.Approved
            };
            var rejectedVacationRequest = new LeaveRequest
            {
                UserId = user.Id,
                LeaveRequestTypeId = vacationType.Id,
                StartDate = new DateTime(2024, 8, 1),
                EndDate = new DateTime(2024, 8, 5),
                RequestStatus = RequestStatus.Rejected
            };

            _context.LeaveRequests.AddRange(vacationRequest, sickRequest, rejectedVacationRequest);
            await _context.SaveChangesAsync();

            // Act - Get vacation requests for 2024
            var result = await _repository.GetLeaveRequestsByUserAndTypeAsync(user.Id, vacationType.Id, 2024);

            // Assert
            result.Should().HaveCount(1);
            result[0].LeaveRequestTypeId.Should().Be(vacationType.Id);
            result[0].RequestStatus.Should().Be(RequestStatus.Approved);
        }

        [Fact]
        public async Task GetAllWithRelationshipsAsync_ShouldReturnAllWithRelationships()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };
            var leaveType = new LeaveRequestType { Title = "Vacation", MaxDays = 25, IsPaid = true };

            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Departments.Add(department);
            _context.JobTitles.Add(jobTitle);
            _context.LeaveRequestTypes.Add(leaveType);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var leaveRequest = new LeaveRequest
            {
                UserId = user.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 5),
                RequestStatus = RequestStatus.Approved
            };

            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllWithRelationshipsAsync();

            // Assert
            result.Should().HaveCount(1);
            result[0].User.Should().NotBeNull();
            result[0].User.Department.Should().NotBeNull();
            result[0].LeaveRequestType.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllWithRelationshipsByUserIdsAsync_ShouldFilterByUserIds()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };
            var leaveType = new LeaveRequestType { Title = "Vacation", MaxDays = 25, IsPaid = true };

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

            _context.Departments.Add(department);
            _context.JobTitles.Add(jobTitle);
            _context.LeaveRequestTypes.Add(leaveType);
            _context.Users.AddRange(user1, user2, user3);
            await _context.SaveChangesAsync();

            var request1 = new LeaveRequest
            {
                UserId = user1.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 5),
                RequestStatus = RequestStatus.Approved
            };
            var request2 = new LeaveRequest
            {
                UserId = user2.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 7, 1),
                EndDate = new DateTime(2024, 7, 3),
                RequestStatus = RequestStatus.Pending
            };
            var request3 = new LeaveRequest
            {
                UserId = user3.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 8, 1),
                EndDate = new DateTime(2024, 8, 2),
                RequestStatus = RequestStatus.Approved
            };

            _context.LeaveRequests.AddRange(request1, request2, request3);
            await _context.SaveChangesAsync();

            var userIds = new List<int> { user1.Id, user2.Id };

            // Act
            var result = await _repository.GetAllWithRelationshipsByUserIdsAsync(userIds);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(lr => lr.UserId == user1.Id);
            result.Should().Contain(lr => lr.UserId == user2.Id);
            result.Should().NotContain(lr => lr.UserId == user3.Id);
        }

        [Fact]
        public async Task GetAllWithRelationshipsByUserIdsAsync_ShouldFilterByName()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };
            var leaveType = new LeaveRequestType { Title = "Vacation", MaxDays = 25, IsPaid = true };

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
            _context.LeaveRequestTypes.Add(leaveType);
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var request1 = new LeaveRequest
            {
                UserId = user1.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 5),
                RequestStatus = RequestStatus.Approved
            };
            var request2 = new LeaveRequest
            {
                UserId = user2.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 7, 1),
                EndDate = new DateTime(2024, 7, 3),
                RequestStatus = RequestStatus.Pending
            };

            _context.LeaveRequests.AddRange(request1, request2);
            await _context.SaveChangesAsync();

            var userIds = new List<int> { user1.Id, user2.Id };

            // Act
            var result = await _repository.GetAllWithRelationshipsByUserIdsAsync(userIds, "John");

            // Assert
            result.Should().HaveCount(1);
            result[0].User.FirstName.Should().Be("John");
        }

        [Fact]
        public async Task GetLeaveRequestWithDetailsAsync_ShouldReturnRequestWithDetails()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };
            var leaveType = new LeaveRequestType { Title = "Vacation", MaxDays = 25, IsPaid = true };

            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };
            var reviewer = new User
            {
                FirstName = "Manager",
                LastName = "Smith",
                Email = "manager.smith@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Departments.Add(department);
            _context.JobTitles.Add(jobTitle);
            _context.LeaveRequestTypes.Add(leaveType);
            _context.Users.AddRange(user, reviewer);
            await _context.SaveChangesAsync();

            var leaveRequest = new LeaveRequest
            {
                UserId = user.Id,
                ReviewerId = reviewer.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 5),
                Reason = "Family vacation",
                RequestStatus = RequestStatus.Approved,
                ReviewerComment = "Approved for vacation"
            };

            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLeaveRequestWithDetailsAsync(leaveRequest.Id);

            // Assert
            result.Should().NotBeNull();
            result.User.Should().NotBeNull();
            result.User.Department.Should().NotBeNull();
            result.LeaveRequestType.Should().NotBeNull();
            result.Reviewer.Should().NotBeNull();
            result.Reason.Should().Be("Family vacation");
            result.ReviewerComment.Should().Be("Approved for vacation");
        }

        [Fact]
        public async Task GetLeaveRequestWithDetailsAsync_ShouldThrowWhenNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _repository.GetLeaveRequestWithDetailsAsync(999));
        }

        [Fact]
        public async Task GetFilteredLeaveRequestsAsync_ShouldReturnFilteredByStatus()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };
            var leaveType = new LeaveRequestType { Title = "Vacation", MaxDays = 25, IsPaid = true };

            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Departments.Add(department);
            _context.JobTitles.Add(jobTitle);
            _context.LeaveRequestTypes.Add(leaveType);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var approvedRequest = new LeaveRequest
            {
                UserId = user.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 5),
                RequestStatus = RequestStatus.Approved,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };
            var pendingRequest = new LeaveRequest
            {
                UserId = user.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 7, 1),
                EndDate = new DateTime(2024, 7, 3),
                RequestStatus = RequestStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _context.LeaveRequests.AddRange(approvedRequest, pendingRequest);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetFilteredLeaveRequestsAsync("Approved", 10, 1);

            // Assert
            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Items[0].RequestStatus.Should().Be(RequestStatus.Approved);
        }

        [Fact]
        public async Task GetFilteredLeaveRequestsAsync_ShouldReturnAllWhenStatusIsAll()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };
            var leaveType = new LeaveRequestType { Title = "Vacation", MaxDays = 25, IsPaid = true };

            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Departments.Add(department);
            _context.JobTitles.Add(jobTitle);
            _context.LeaveRequestTypes.Add(leaveType);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var approvedRequest = new LeaveRequest
            {
                UserId = user.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 5),
                RequestStatus = RequestStatus.Approved,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };
            var pendingRequest = new LeaveRequest
            {
                UserId = user.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 7, 1),
                EndDate = new DateTime(2024, 7, 3),
                RequestStatus = RequestStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _context.LeaveRequests.AddRange(approvedRequest, pendingRequest);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetFilteredLeaveRequestsAsync("ALL", 10, 1);

            // Assert
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetFilteredLeaveRequestsAsync_ShouldFilterByEmployeeIds()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };
            var leaveType = new LeaveRequestType { Title = "Vacation", MaxDays = 25, IsPaid = true };

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
            _context.LeaveRequestTypes.Add(leaveType);
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var request1 = new LeaveRequest
            {
                UserId = user1.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 5),
                RequestStatus = RequestStatus.Approved,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };
            var request2 = new LeaveRequest
            {
                UserId = user2.Id,
                LeaveRequestTypeId = leaveType.Id,
                StartDate = new DateTime(2024, 7, 1),
                EndDate = new DateTime(2024, 7, 3),
                RequestStatus = RequestStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _context.LeaveRequests.AddRange(request1, request2);
            await _context.SaveChangesAsync();

            var employeeIds = new List<int> { user1.Id };

            // Act
            var result = await _repository.GetFilteredLeaveRequestsAsync("ALL", 10, 1, employeeIds);

            // Assert
            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Items[0].UserId.Should().Be(user1.Id);
        }

        [Fact]
        public async Task GetFilteredLeaveRequestsAsync_ShouldApplyPagination()
        {
            // Arrange
            var department = new Department { Name = "IT", Description = "Information Technology" };
            var jobTitle = new JobTitle { Name = "Developer" };
            var leaveType = new LeaveRequestType { Title = "Vacation", MaxDays = 25, IsPaid = true };

            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Departments.Add(department);
            _context.JobTitles.Add(jobTitle);
            _context.LeaveRequestTypes.Add(leaveType);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var requests = new List<LeaveRequest>();
            for (int i = 1; i <= 5; i++)
            {
                requests.Add(new LeaveRequest
                {
                    UserId = user.Id,
                    LeaveRequestTypeId = leaveType.Id,
                    StartDate = new DateTime(2024, i, 1),
                    EndDate = new DateTime(2024, i, 2),
                    RequestStatus = RequestStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddDays(-i)
                });
            }

            _context.LeaveRequests.AddRange(requests);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetFilteredLeaveRequestsAsync("ALL", 2, 2);

            // Assert
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(5);
        }

        [Fact]
        public async Task GetFilteredLeaveRequestsAsync_ShouldThrowForInvalidStatus()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _repository.GetFilteredLeaveRequestsAsync("InvalidStatus", 10, 1));
        }

        [Fact]
        public async Task GetFilteredLeaveRequestsAsync_ShouldThrowForInvalidPageNumber()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _repository.GetFilteredLeaveRequestsAsync("ALL", 10, 0));
        }

        [Fact]
        public async Task GetFilteredLeaveRequestsAsync_ShouldThrowForInvalidPageSize()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _repository.GetFilteredLeaveRequestsAsync("ALL", 0, 1));
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
