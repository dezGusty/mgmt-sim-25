using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementSimulator.Core.Services;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories.Intefaces;
using ManagementSimulator.Database.Repositories.Interfaces;
using NSubstitute;
using Xunit;

namespace ManagementSimulator.Tests.Services
{
	public class ResourceAuthorizationServiceTests
	{
		private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
		private readonly ILeaveRequestRepository _leaveRepo = Substitute.For<ILeaveRequestRepository>();
		private readonly ISecondManagerRepository _secondMgrRepo = Substitute.For<ISecondManagerRepository>();

		private ResourceAuthorizationService CreateServ() => new ResourceAuthorizationService(_userRepo, _leaveRepo, _secondMgrRepo);

		[Fact]
		public async Task CanManagerAccessUserDataAsync_Should_Return_True_For_Subordinate()
		{
			_userRepo.GetUsersByManagerIdAsync(100, Arg.Any<bool>()).Returns(new List<User>
			{
				new User { Id = 200 },
				new User { Id = 201 }
			});

			var serv = CreateServ();
			var canAccess = await serv.CanManagerAccessUserDataAsync(100, 200);
			canAccess.Should().BeTrue();
			var canAccess2 = await serv.CanManagerAccessUserDataAsync(100, 201);
			canAccess.Should().BeTrue();
		}

		[Fact]
		public async Task CanManagerAccessLeaveRequestAsync_Should_Check_Ownership()
		{
			_leaveRepo.GetFirstOrDefaultAsync(7, Arg.Any<bool>()).Returns(new LeaveRequest { Id = 7, UserId = 300 });
			_userRepo.GetUsersByManagerIdAsync(150, Arg.Any<bool>()).Returns(new List<User> { new User { Id = 300 } });

			var serv = CreateServ();
			var canAccess = await serv.CanManagerAccessLeaveRequestAsync(150, 7);
			canAccess.Should().BeTrue();
		}

		[Fact]
		public async Task CanUserAccessOwnLeaveRequestAsync_Should_Return_True_For_Owner()
		{
			_leaveRepo.GetFirstOrDefaultAsync(9, Arg.Any<bool>()).Returns(new LeaveRequest { Id = 9, UserId = 400 });

			var serv = CreateServ();
			var canAccess = await serv.CanUserAccessOwnLeaveRequestAsync(400, 9);
			canAccess.Should().BeTrue();
		}

		[Fact]
		public async Task CanManagerModifyDataAsync_Should_Be_False_When_Temporarily_Replaced()
		{
			_secondMgrRepo.GetActiveSecondManagersAsync().Returns(new List<SecondManager>
			{
				new SecondManager{ SecondManagerEmployeeId = 501, ReplacedManagerId = 500, StartDate = System.DateTime.Now.AddDays(-1), EndDate = System.DateTime.Now.AddDays(1) }
			});

			var serv = CreateServ();
			var canModify = await serv.CanManagerModifyDataAsync(500);
			canModify.Should().BeFalse();
		}

		[Fact]
		public async Task GetActiveSecondManagerForManagerAsync_Should_Return_Id()
		{
			_secondMgrRepo.GetActiveSecondManagersAsync().Returns(new List<SecondManager>
			{
				new SecondManager{ SecondManagerEmployeeId = 777, ReplacedManagerId = 600, StartDate = System.DateTime.Now.AddDays(-1), EndDate = System.DateTime.Now.AddDays(1) }
			});

			var serv = CreateServ();
			var secondManagerId = await serv.GetActiveSecondManagerForManagerAsync(600);
			secondManagerId.Should().Be(777);
		}

		[Fact]
		public async Task IsUserActingAsSecondManagerAsync_Should_Return_True_When_List_Contains_User()
		{
			_secondMgrRepo.GetActiveSecondManagersAsync().Returns(new List<SecondManager>
			{
				new SecondManager{ SecondManagerEmployeeId = 888, ReplacedManagerId = 601, StartDate = System.DateTime.Now.AddDays(-1), EndDate = System.DateTime.Now.AddDays(1) }
			});

			var serv = CreateServ();
			var isSecondManager = await serv.IsUserActingAsSecondManagerAsync(888);
			isSecondManager.Should().BeTrue();
		}
	}
}