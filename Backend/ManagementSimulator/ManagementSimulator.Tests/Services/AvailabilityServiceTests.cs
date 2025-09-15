using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementSimulator.Core.Services;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Enums;
using ManagementSimulator.Database.Repositories.Intefaces;
using NSubstitute;
using Xunit;

namespace ManagementSimulator.Tests.Services
{
	public class AvailabilityServiceTests
	{
		private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
		private readonly IProjectRepository _projectRepo = Substitute.For<IProjectRepository>();

		private AvailabilityService CreateServ() => new AvailabilityService(_userRepo, _projectRepo);

		[Theory]
		[InlineData(EmploymentType.FullTime, 1.0f)]
		[InlineData(EmploymentType.PartTime, 0.5f)]
		public void CalculateTotalAvailability_Should_Return_Correct_FTE(EmploymentType type, float expected)
		{
			var serv = CreateServ();
			serv.CalculateTotalAvailability(type).Should().Be(expected);
		}

		[Fact]
		public async Task CalculateRemainingAvailabilityAsync_Should_Consider_Project_Allocations()
		{
			var serv = CreateServ();
			var user = new User { Id = 1, EmploymentType = EmploymentType.FullTime };
			_userRepo.GetFirstOrDefaultAsync(1).Returns(user);
			_projectRepo.GetUserProjectsByUserIdAsync(1).Returns(new List<UserProject>
			{
				new UserProject{ UserId = 1, ProjectId = 10, TimePercentagePerProject = 30 },
				new UserProject{ UserId = 1, ProjectId = 11, TimePercentagePerProject = 50 }
			});

			var remaining = await serv.CalculateRemainingAvailabilityAsync(1);

			remaining.Should().BeApproximately(0.2f, 0.0001f);
		}

		[Fact]
		public async Task UpdateUserAvailabilityAsync_Should_Update_And_Save()
		{
			var serv = CreateServ();
			var user = new User { Id = 2, EmploymentType = EmploymentType.PartTime };
			_userRepo.GetFirstOrDefaultAsync(2).Returns(user);
			_projectRepo.GetUserProjectsByUserIdAsync(2).Returns(new List<UserProject>
			{
				new UserProject{ UserId = 2, ProjectId = 20, TimePercentagePerProject = 50 },
			});

			var ok = await serv.UpdateUserAvailabilityAsync(2);

			ok.Should().BeTrue();
			user.TotalAvailability.Should().Be(0.5f);
			user.RemainingAvailability.Should().BeApproximately(0.25f, 0.0001f);
			await _userRepo.Received(1).SaveChangesAsync();
		}

		[Fact]
		public async Task ValidateProjectAssignmentAsync_Should_Block_When_Exceeds_Total()
		{
			var serv = CreateServ();
			var user = new User { Id = 3, EmploymentType = EmploymentType.FullTime };
			_userRepo.GetFirstOrDefaultAsync(3).Returns(user);
			_projectRepo.GetUserProjectsByUserIdAsync(3).Returns(new List<UserProject>
			{
				new UserProject{ UserId = 3, ProjectId = 30, TimePercentagePerProject = 60 },
				new UserProject{ UserId = 3, ProjectId = 31, TimePercentagePerProject = 30 }
			});

			var isValid = await serv.ValidateProjectAssignmentAsync(3, 20);
			isValid.Should().BeFalse();
		}
	}
}