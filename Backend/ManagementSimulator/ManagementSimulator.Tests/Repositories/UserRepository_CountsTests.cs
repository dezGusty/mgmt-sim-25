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

			var a1 = new User { Email = "Arhire@simulator.com", FirstName = "Arhire", LastName = "Alpaca", PasswordHash = "x", Department = dept, Title = title };
			var a2 = new User { Email = "Andrusca@simulator.com", FirstName = "Andrusca", LastName = "Bondar", PasswordHash = "x", Department = dept, Title = title };
			var u1 = new User { Email = "Ulea@simulator.com", FirstName = "Ulea", LastName = "Corvett", PasswordHash = "x", Department = dept, Title = title };
			ctx.Users.AddRange(a1, a2, u1);
			await ctx.SaveChangesAsync();

			ctx.EmployeeRolesUsers.Add(new EmployeeRoleUser { Role = adminRole, User = a1 });
			ctx.EmployeeRolesUsers.Add(new EmployeeRoleUser { Role = adminRole, User = a2 });
			ctx.EmployeeRolesUsers.Add(new EmployeeRoleUser { Role = userRole, User = u1 });
			await ctx.SaveChangesAsync();

			var repo = new UserRepository(ctx, new TestAuditService());
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

			var m1 = new User { Email = "Mitica@simulator.com", FirstName = "Mitica", LastName = "Botez", PasswordHash = "x", Department = dept, Title = title };
			var m2 = new User { Email = "Marean@simulator.com", FirstName = "Marean", LastName = "Londonezu", PasswordHash = "x", Department = dept, Title = title };
			var e1 = new User { Email = "Eugustin@simulator.com", FirstName = "Eugustin", LastName = "Amigo", PasswordHash = "x", Department = dept, Title = title };
			ctx.Users.AddRange(m1, m2, e1);
			await ctx.SaveChangesAsync();

			ctx.EmployeeManagers.Add(new EmployeeManager { Employee = e1, EmployeeId = e1.Id, Manager = m1, ManagerId = m1.Id });
			await ctx.SaveChangesAsync();

			var repo = new UserRepository(ctx, new TestAuditService());
			var count = await repo.GetTotalManagersCountAsync();
			count.Should().Be(1);
		}
	}
}
