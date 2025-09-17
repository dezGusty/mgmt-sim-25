using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementSimulator.Core.Dtos.Requests.LeaveRequest;
using ManagementSimulator.Core.Dtos.Requests.LeaveRequests;
using ManagementSimulator.Core.Dtos.Responses.PublicHolidays;
using ManagementSimulator.Core.Services;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Enums;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Infrastructure.Exceptions;
using NSubstitute;
using Xunit;

namespace ManagementSimulator.Tests.Services
{
	public class LeaveRequestServiceTests
	{
		private readonly ILeaveRequestRepository _leaveRepo = Substitute.For<ILeaveRequestRepository>();
		private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
		private readonly ILeaveRequestTypeRepository _typeRepo = Substitute.For<ILeaveRequestTypeRepository>();
		private readonly IEmployeeManagerService _empMgrSvc = Substitute.For<IEmployeeManagerService>();
		private readonly IEmailService _emailSvc = Substitute.For<IEmailService>();
		private readonly IPublicHolidayService _publicHolidaySvc = Substitute.For<IPublicHolidayService>();

		private LeaveRequestService CreateServ()
		{
			// Setup default mocks for public holiday service
			_publicHolidaySvc.GetHolidaysInRangeAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
				.Returns(new List<PublicHolidayResponseDto>());

			return new LeaveRequestService(_leaveRepo, _userRepo, _typeRepo, _empMgrSvc, _emailSvc, _publicHolidaySvc);
		}
		[Fact]
		public async Task AddLeaveRequestAsync_Should_Create_Pending_Request_When_Valid()
		{
			var serv = CreateServ();
			var user = new User { Id = 10, FirstName = "Michael", LastName = "Jekson", Vacation = 21 };
			var type = new LeaveRequestType { Id = 5, Title = "Vacation", MaxDays = 21, IsPaid = false };
			_userRepo.GetFirstOrDefaultAsync(user.Id).Returns(user);
			_typeRepo.GetFirstOrDefaultAsync(type.Id).Returns(type);
			_leaveRepo.GetOverlappingRequestsAsync(user.Id, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<bool>())
				.Returns(new List<LeaveRequest>());
			_leaveRepo.GetLeaveRequestsByUserAndTypeAsync(user.Id, type.Id, Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<bool>())
				.Returns(new List<LeaveRequest>());
			_leaveRepo.AddAsync(Arg.Any<LeaveRequest>()).Returns(ci => ci.Arg<LeaveRequest>());
			_leaveRepo.GetLeaveRequestWithDetailsAsync(Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<bool>())
				.Returns(ci =>
				{
					var lr = new LeaveRequest
					{
						Id = 123,
						UserId = user.Id,
						User = user,
						LeaveRequestTypeId = type.Id,
						LeaveRequestType = type,
						StartDate = new DateTime(2025, 1, 6),
						EndDate = new DateTime(2025, 1, 10),
						RequestStatus = RequestStatus.Pending,
					};
					return lr;
				});

			var dto = new CreateLeaveRequestRequestDto
			{
				UserId = user.Id,
				LeaveRequestTypeId = type.Id,
				StartDate = new DateTime(2025, 1, 6),
				EndDate = new DateTime(2025, 1, 10)
			};

			var result = await serv.AddLeaveRequestAsync(dto);

			result.Id.Should().Be(123);
			result.RequestStatus.Should().Be(RequestStatus.Pending);
			result.LeaveRequestTypeId.Should().Be(type.Id);
			result.LeaveRequestTypeIsPaid.Should().BeFalse();
			await _leaveRepo.Received(1).AddAsync(Arg.Any<LeaveRequest>());
		}

		[Fact]
		public async Task AddLeaveRequestAsync_Should_Throw_When_User_Not_Found()
		{
			var serv = CreateServ();
			_userRepo.GetFirstOrDefaultAsync(Arg.Any<int>()).Returns((User?)null);

			var dto = new CreateLeaveRequestRequestDto
			{
				UserId = 999,
				LeaveRequestTypeId = 1,
				StartDate = DateTime.Today,
				EndDate = DateTime.Today.AddDays(1),
			};

			await Assert.ThrowsAsync<EntryNotFoundException>(() => serv.AddLeaveRequestAsync(dto));
		}
		[Fact]
		public async Task AddLeaveRequestAsync_Should_Throw_On_Invalid_Date_Range()
		{
			var serv = CreateServ();
			_userRepo.GetFirstOrDefaultAsync(Arg.Any<int>()).Returns(new User { Id = 1 });
			_typeRepo.GetFirstOrDefaultAsync(Arg.Any<int>()).Returns(new LeaveRequestType { Id = 1, Title = "Vacation", MaxDays = 21 });

			var dto = new CreateLeaveRequestRequestDto
			{
				UserId = 1,
				LeaveRequestTypeId = 1,
				StartDate = new DateTime(2025, 1, 10),
				EndDate = new DateTime(2025, 1, 6)
			};

			await Assert.ThrowsAsync<InvalidDateRangeException>(() => serv.AddLeaveRequestAsync(dto));
		}

		[Fact]
		public async Task AddLeaveRequestAsync_Should_Throw_On_Overlapping_Request()
		{
			var serv = CreateServ();
			_userRepo.GetFirstOrDefaultAsync(Arg.Any<int>()).Returns(new User { Id = 1 });
			_typeRepo.GetFirstOrDefaultAsync(Arg.Any<int>()).Returns(new LeaveRequestType { Id = 1, Title = "Vacation", MaxDays = 21, IsPaid = false });
			_leaveRepo.GetOverlappingRequestsAsync(1, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<bool>())
				.Returns(new List<LeaveRequest>
				{
					new LeaveRequest{ UserId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today, RequestStatus = RequestStatus.Pending }
				});

			var dto = new CreateLeaveRequestRequestDto
			{
				UserId = 1,
				LeaveRequestTypeId = 1,
				StartDate = DateTime.Today,
				EndDate = DateTime.Today
			};

			await Assert.ThrowsAsync<LeaveRequestOverlapException>(() => serv.AddLeaveRequestAsync(dto));
		}

		[Fact]
		public async Task CancelLeaveRequestAsync_Should_Set_Status_To_Canceled_When_Pending_And_Owner()
		{
			var serv = CreateServ();
			var lr = new LeaveRequest
			{
				Id = 7,
				UserId = 22,
				StartDate = DateTime.Today,
				EndDate = DateTime.Today.AddDays(1),
				RequestStatus = RequestStatus.Pending
			};
			_leaveRepo.GetFirstOrDefaultAsync(lr.Id, Arg.Any<bool>()).Returns(lr);

			await serv.CancelLeaveRequestAsync(lr.Id, 22);

			lr.RequestStatus.Should().Be(RequestStatus.Canceled);
			await _leaveRepo.Received(1).UpdateAsync(Arg.Is<LeaveRequest>(x => x.Id == 7 && x.RequestStatus == RequestStatus.Canceled));
		}

		[Fact]
		public async Task GetRemainingLeaveDaysAsync_Should_Compute_Remaining_For_Vacation()
		{
			var serv = CreateServ();
			var user = new User { Id = 10, FirstName = "Ionake", LastName = "Banana", Vacation = 21 };
			var type = new LeaveRequestType { Id = 3, Title = "Vacation", MaxDays = 21 };
			_userRepo.GetFirstOrDefaultAsync(user.Id).Returns(user);
			_typeRepo.GetFirstOrDefaultAsync(type.Id).Returns(type);
			_leaveRepo.GetLeaveRequestsByUserAndTypeAsync(user.Id, type.Id, Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<bool>())
				.Returns(new List<LeaveRequest>
				{
					new LeaveRequest{ StartDate = new DateTime(2025,1,6), EndDate = new DateTime(2025,1,10), RequestStatus = RequestStatus.Approved },
				});

			var response = await serv.GetRemainingLeaveDaysAsync(user.Id, type.Id, 2025);

			response.MaxDaysAllowed.Should().Be(21);
			response.DaysUsed.Should().Be(5);
			response.RemainingDays.Should().Be(16);
			response.HasUnlimitedDays.Should().BeFalse();
		}
	}
}