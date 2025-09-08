using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories;
using Xunit;

namespace ManagementSimulator.Tests.Repositories
{
	public class UserRepository_ManagerTests
	{
		private MGMTSimulatorDbContext CreateContext() => DbContextFactory.CreateInMemoryContext();

		[Fact]
		public async Task GetUsersByManagerIdAsync_Should_Return_Subordinates()
		{
			var ctx = CreateContext();
			var dept = new Department { Name = "Dev" };
			var title = new JobTitle { Name = "Engineer" };
			ctx.Departments.Add(dept);
			ctx.JobTitles.Add(title);
			await ctx.SaveChangesAsync();

			var manager = new User { Email = "m@x.com", FirstName = "M", LastName = "Boss", PasswordHash = "x", Department = dept, Title = title };
			var e1 = new User { Email = "e1@x.com", FirstName = "E1", LastName = "A", PasswordHash = "x", Department = dept, Title = title };
			var e2 = new User { Email = "e2@x.com", FirstName = "E2", LastName = "B", PasswordHash = "x", Department = dept, Title = title };
			ctx.Users.AddRange(manager, e1, e2);
			await ctx.SaveChangesAsync();

			ctx.EmployeeManagers.Add(new EmployeeManager { EmployeeId = e1.Id, ManagerId = manager.Id, Employee = e1, Manager = manager });
			ctx.EmployeeManagers.Add(new EmployeeManager { EmployeeId = e2.Id, ManagerId = manager.Id, Employee = e2, Manager = manager });
			await ctx.SaveChangesAsync();

			var repo = new UserRepository(ctx);
			var subs = await repo.GetUsersByManagerIdAsync(manager.Id);

			subs.Should().HaveCount(2);
			subs.Select(s => s.Email).Should().BeEquivalentTo(new[] { "e1@x.com", "e2@x.com" });
		}

		[Fact]
		public async Task GetManagersByUserIdsAsync_Should_Load_Managers_For_Employees()
		{
			var ctx = CreateContext();
			var dept = new Department { Name = "Ops" };
			var title = new JobTitle { Name = "Tech" };
			ctx.Departments.Add(dept);
			ctx.JobTitles.Add(title);
			await ctx.SaveChangesAsync();

			var m1 = new User { Email = "m1@x.com", FirstName = "M1", LastName = "Boss", PasswordHash = "x", Department = dept, Title = title };
			var m2 = new User { Email = "m2@x.com", FirstName = "M2", LastName = "Lead", PasswordHash = "x", Department = dept, Title = title };
			var e1 = new User { Email = "e1@x.com", FirstName = "E1", LastName = "A", PasswordHash = "x", Department = dept, Title = title };
			ctx.Users.AddRange(m1, m2, e1);
			await ctx.SaveChangesAsync();

			ctx.EmployeeManagers.Add(new EmployeeManager { EmployeeId = e1.Id, ManagerId = m1.Id, Employee = e1, Manager = m1 });
			ctx.EmployeeManagers.Add(new EmployeeManager { EmployeeId = e1.Id, ManagerId = m2.Id, Employee = e1, Manager = m2 });
			await ctx.SaveChangesAsync();

			var repo = new UserRepository(ctx);
			var employees = await repo.GetManagersByUserIdsAsync(new List<int> { e1.Id });

			employees.Should().HaveCount(1);
			var loaded = employees[0];
			loaded.Managers.Should().HaveCount(2);
			loaded.Managers.Select(em => em.Manager.Email).Should().BeEquivalentTo(new[] { "m1@x.com", "m2@x.com" });
		}
	}
}
