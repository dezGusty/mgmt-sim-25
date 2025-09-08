using System.Threading.Tasks;
using FluentAssertions;
using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories;
using Xunit;

namespace ManagementSimulator.Tests.Repositories
{
	public class UserRepository_CountsTests
	{
		private MGMTSimulatorDbContext CreateContext() => DbContextFactory.CreateInMemoryContext();

		[Fact]
		public async Task GetTotalAdminsCountAsync_Should_Count_Only_Admins()
		{
			var ctx = CreateContext();
			var dept = new Department { Name = "AdminDept" };
			var title = new JobTitle { Name = "Dev" };
			var adminRole = new EmployeeRole { Rolename = "Admin" };
			var userRole = new EmployeeRole { Rolename = "User" };
			ctx.AddRange(dept, title, adminRole, userRole);
			await ctx.SaveChangesAsync();

			var a1 = new User { Email = "a1@x.com", FirstName = "A1", LastName = "A", PasswordHash = "x", Department = dept, Title = title };
			var a2 = new User { Email = "a2@x.com", FirstName = "A2", LastName = "B", PasswordHash = "x", Department = dept, Title = title };
			var u1 = new User { Email = "u1@x.com", FirstName = "U1", LastName = "C", PasswordHash = "x", Department = dept, Title = title };
			ctx.Users.AddRange(a1, a2, u1);
			await ctx.SaveChangesAsync();

			ctx.EmployeeRolesUsers.Add(new EmployeeRoleUser { Role = adminRole, User = a1 });
			ctx.EmployeeRolesUsers.Add(new EmployeeRoleUser { Role = adminRole, User = a2 });
			ctx.EmployeeRolesUsers.Add(new EmployeeRoleUser { Role = userRole, User = u1 });
			await ctx.SaveChangesAsync();

			var repo = new UserRepository(ctx);
			var count = await repo.GetTotalAdminsCountAsync();
			count.Should().Be(2);
		}

		[Fact]
		public async Task GetTotalManagersCountAsync_Should_Count_Users_With_Subordinates()
		{
			var ctx = CreateContext();
			var dept = new Department { Name = "OpsDept" };
			var title = new JobTitle { Name = "Tech" };
			ctx.AddRange(dept, title);
			await ctx.SaveChangesAsync();

			var m1 = new User { Email = "m1@x.com", FirstName = "M1", LastName = "Boss", PasswordHash = "x", Department = dept, Title = title };
			var m2 = new User { Email = "m2@x.com", FirstName = "M2", LastName = "Lead", PasswordHash = "x", Department = dept, Title = title };
			var e1 = new User { Email = "e1@x.com", FirstName = "E1", LastName = "A", PasswordHash = "x", Department = dept, Title = title };
			ctx.Users.AddRange(m1, m2, e1);
			await ctx.SaveChangesAsync();

			ctx.EmployeeManagers.Add(new EmployeeManager { Employee = e1, EmployeeId = e1.Id, Manager = m1, ManagerId = m1.Id });
			await ctx.SaveChangesAsync();

			var repo = new UserRepository(ctx);
			var count = await repo.GetTotalManagersCountAsync();
			count.Should().Be(1);
		}
	}
}
