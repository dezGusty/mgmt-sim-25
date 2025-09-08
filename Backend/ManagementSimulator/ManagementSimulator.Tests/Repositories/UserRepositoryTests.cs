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
	public class UserRepositoryTests
	{
		private MGMTSimulatorDbContext CreateContext() => DbContextFactory.CreateInMemoryContext();

		[Fact]
		public async Task GetAllAdminsAsync_Should_Return_Only_Admins_With_Filters()
		{
			var ctx = CreateContext();
			var dept = new Department { Name = "HQ" };
			var title = new JobTitle { Name = "Dev" };
			var adminRole = new EmployeeRole { Rolename = "Admin" };
			var managerRole = new EmployeeRole { Rolename = "Manager" };
			ctx.Departments.Add(dept);
			ctx.JobTitles.Add(title);
			ctx.EmployeeRoles.AddRange(adminRole, managerRole);
			await ctx.SaveChangesAsync();

			var u1 = new User { Email = "admin1@simulator.com", FirstName = "Ana", LastName = "Ioneascu", PasswordHash = "x", Department = dept, Title = title };
			var u2 = new User { Email = "manager1@simulator.com", FirstName = "Mihai", LastName = "Bogdan", PasswordHash = "x", Department = dept, Title = title };
			var u3 = new User { Email = "admin2@simulator.com", FirstName = "Alex", LastName = "NNiculaescu", PasswordHash = "x", Department = dept, Title = title };
			ctx.Users.AddRange(u1, u2, u3);
			await ctx.SaveChangesAsync();

			ctx.EmployeeRolesUsers.Add(new EmployeeRoleUser { Role = adminRole, User = u1 });
			ctx.EmployeeRolesUsers.Add(new EmployeeRoleUser { Role = managerRole, User = u2 });
			ctx.EmployeeRolesUsers.Add(new EmployeeRoleUser { Role = adminRole, User = u3 });
			await ctx.SaveChangesAsync();

			var repo = new UserRepository(ctx);
			var admins = await repo.GetAllAdminsAsync(name: "A", email: null);

			admins.Should().HaveCount(2);
			admins.Select(a => a.Email).Should().BeEquivalentTo(new[] { "admin1@simulator.com", "admin2@simulator.com" });

			var adminsByEmail = await repo.GetAllAdminsAsync(name: null, email: "admin2");
			adminsByEmail.Should().ContainSingle();
			adminsByEmail[0].Email.Should().Be("admin2@simulator.com");
		}
	}
}
