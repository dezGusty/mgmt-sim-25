using System.Threading.Tasks;
using FluentAssertions;
using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories;
using Xunit;
using System;

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

		[Fact]
		public async Task GetTotalAdminsCountAsync_Should_Return_Zero_When_No_Admins_Exist()
		{
			var ctx = CreateContext();
			var dept = new Department { Name = "TestDept" };
			var title = new JobTitle { Name = "TestTitle" };
			var userRole = new EmployeeRole { Rolename = "User" };
			ctx.AddRange(dept, title, userRole);
			await ctx.SaveChangesAsync();

			var user = new User { Email = "test@simulator.com", FirstName = "Test", LastName = "User", PasswordHash = "x", Department = dept, Title = title };
			ctx.Users.Add(user);
			await ctx.SaveChangesAsync();

			ctx.EmployeeRolesUsers.Add(new EmployeeRoleUser { Role = userRole, User = user });
			await ctx.SaveChangesAsync();

			var repo = new UserRepository(ctx);
			var count = await repo.GetTotalAdminsCountAsync();
			count.Should().Be(0);
		}

		[Fact]
		public async Task GetTotalAdminsCountAsync_Should_Return_Zero_When_Database_Is_Empty()
		{
			var ctx = CreateContext();
			var repo = new UserRepository(ctx);
			var count = await repo.GetTotalAdminsCountAsync();
			count.Should().Be(0);
		}

		[Fact]
		public async Task GetTotalAdminsCountAsync_Should_Exclude_Deleted_Admins_By_Default()
		{
			var ctx = CreateContext();
			var dept = new Department { Name = "AdminDept" };
			var title = new JobTitle { Name = "Dev" };
			var adminRole = new EmployeeRole { Rolename = "Admin" };
			ctx.AddRange(dept, title, adminRole);
			await ctx.SaveChangesAsync();

			var activeAdmin = new User { Email = "active@simulator.com", FirstName = "Active", LastName = "Admin", PasswordHash = "x", Department = dept, Title = title };
			var deletedAdmin = new User { Email = "deleted@simulator.com", FirstName = "Deleted", LastName = "Admin", PasswordHash = "x", Department = dept, Title = title, DeletedAt = DateTime.UtcNow };
			ctx.Users.AddRange(activeAdmin, deletedAdmin);
			await ctx.SaveChangesAsync();

			ctx.EmployeeRolesUsers.Add(new EmployeeRoleUser { Role = adminRole, User = activeAdmin });
			ctx.EmployeeRolesUsers.Add(new EmployeeRoleUser { Role = adminRole, User = deletedAdmin });
			await ctx.SaveChangesAsync();

			var repo = new UserRepository(ctx);
			var count = await repo.GetTotalAdminsCountAsync();
			count.Should().Be(1);
		}

		[Fact]
		public async Task GetTotalAdminsCountAsync_Should_Include_Deleted_Admins_When_Requested()
		{
			var ctx = CreateContext();
			var dept = new Department { Name = "AdminDept" };
			var title = new JobTitle { Name = "Dev" };
			var adminRole = new EmployeeRole { Rolename = "Admin" };
			ctx.AddRange(dept, title, adminRole);
			await ctx.SaveChangesAsync();

			var activeAdmin = new User { Email = "active@simulator.com", FirstName = "Active", LastName = "Admin", PasswordHash = "x", Department = dept, Title = title };
			var deletedAdmin = new User { Email = "deleted@simulator.com", FirstName = "Deleted", LastName = "Admin", PasswordHash = "x", Department = dept, Title = title, DeletedAt = DateTime.UtcNow };
			ctx.Users.AddRange(activeAdmin, deletedAdmin);
			await ctx.SaveChangesAsync();

			ctx.EmployeeRolesUsers.Add(new EmployeeRoleUser { Role = adminRole, User = activeAdmin });
			ctx.EmployeeRolesUsers.Add(new EmployeeRoleUser { Role = adminRole, User = deletedAdmin });
			await ctx.SaveChangesAsync();

			var repo = new UserRepository(ctx);
			var count = await repo.GetTotalAdminsCountAsync(includeDeleted: true);
			count.Should().Be(2);
		}

		[Fact]
		public async Task GetTotalAdminsCountAsync_Should_Exclude_Users_With_Deleted_Admin_Roles()
		{
			var ctx = CreateContext();
			var dept = new Department { Name = "AdminDept" };
			var title = new JobTitle { Name = "Dev" };
			var adminRole = new EmployeeRole { Rolename = "Admin" };
			ctx.AddRange(dept, title, adminRole);
			await ctx.SaveChangesAsync();

			var user = new User { Email = "user@simulator.com", FirstName = "Test", LastName = "User", PasswordHash = "x", Department = dept, Title = title };
			ctx.Users.Add(user);
			await ctx.SaveChangesAsync();

			ctx.EmployeeRolesUsers.Add(new EmployeeRoleUser { Role = adminRole, User = user, DeletedAt = DateTime.UtcNow });
			await ctx.SaveChangesAsync();

			var repo = new UserRepository(ctx);
			var count = await repo.GetTotalAdminsCountAsync();
			count.Should().Be(0);
		}

		[Fact]
		public async Task GetTotalAdminsCountAsync_Should_Count_Users_With_Multiple_Roles_Including_Admin()
		{
			var ctx = CreateContext();
			var dept = new Department { Name = "AdminDept" };
			var title = new JobTitle { Name = "Dev" };
			var adminRole = new EmployeeRole { Rolename = "Admin" };
			var managerRole = new EmployeeRole { Rolename = "Manager" };
			ctx.AddRange(dept, title, adminRole, managerRole);
			await ctx.SaveChangesAsync();

			var user = new User { Email = "multi@simulator.com", FirstName = "Multi", LastName = "Role", PasswordHash = "x", Department = dept, Title = title };
			ctx.Users.Add(user);
			await ctx.SaveChangesAsync();

			ctx.EmployeeRolesUsers.Add(new EmployeeRoleUser { Role = adminRole, User = user });
			ctx.EmployeeRolesUsers.Add(new EmployeeRoleUser { Role = managerRole, User = user });
			await ctx.SaveChangesAsync();

			var repo = new UserRepository(ctx);
			var count = await repo.GetTotalAdminsCountAsync();
			count.Should().Be(1);
		}

		[Fact]
		public async Task GetTotalManagersCountAsync_Should_Return_Zero_When_No_Managers_Exist()
		{
			var ctx = CreateContext();
			var dept = new Department { Name = "TestDept" };
			var title = new JobTitle { Name = "TestTitle" };
			ctx.AddRange(dept, title);
			await ctx.SaveChangesAsync();

			var user = new User { Email = "test@simulator.com", FirstName = "Test", LastName = "User", PasswordHash = "x", Department = dept, Title = title };
			ctx.Users.Add(user);
			await ctx.SaveChangesAsync();

			var repo = new UserRepository(ctx);
			var count = await repo.GetTotalManagersCountAsync();
			count.Should().Be(0);
		}

		[Fact]
		public async Task GetTotalManagersCountAsync_Should_Return_Zero_When_Database_Is_Empty()
		{
			var ctx = CreateContext();
			var repo = new UserRepository(ctx);
			var count = await repo.GetTotalManagersCountAsync();
			count.Should().Be(0);
		}

		[Fact]
		public async Task GetTotalManagersCountAsync_Should_Exclude_Deleted_Managers_By_Default()
		{
			var ctx = CreateContext();
			var dept = new Department { Name = "OpsDept" };
			var title = new JobTitle { Name = "Tech" };
			ctx.AddRange(dept, title);
			await ctx.SaveChangesAsync();

			var activeManager = new User { Email = "active@simulator.com", FirstName = "Active", LastName = "Manager", PasswordHash = "x", Department = dept, Title = title };
			var deletedManager = new User { Email = "deleted@simulator.com", FirstName = "Deleted", LastName = "Manager", PasswordHash = "x", Department = dept, Title = title, DeletedAt = DateTime.UtcNow };
			var employee = new User { Email = "emp@simulator.com", FirstName = "Employee", LastName = "Test", PasswordHash = "x", Department = dept, Title = title };
			ctx.Users.AddRange(activeManager, deletedManager, employee);
			await ctx.SaveChangesAsync();

			ctx.EmployeeManagers.Add(new EmployeeManager { Employee = employee, EmployeeId = employee.Id, Manager = activeManager, ManagerId = activeManager.Id });
			ctx.EmployeeManagers.Add(new EmployeeManager { Employee = employee, EmployeeId = employee.Id, Manager = deletedManager, ManagerId = deletedManager.Id });
			await ctx.SaveChangesAsync();

			var repo = new UserRepository(ctx);
			var count = await repo.GetTotalManagersCountAsync();
			count.Should().Be(1);
		}

		[Fact]
		public async Task GetTotalManagersCountAsync_Should_Include_Deleted_Managers_When_Requested()
		{
			var ctx = CreateContext();
			var dept = new Department { Name = "OpsDept" };
			var title = new JobTitle { Name = "Tech" };
			ctx.AddRange(dept, title);
			await ctx.SaveChangesAsync();

			var activeManager = new User { Email = "active@simulator.com", FirstName = "Active", LastName = "Manager", PasswordHash = "x", Department = dept, Title = title };
			var deletedManager = new User { Email = "deleted@simulator.com", FirstName = "Deleted", LastName = "Manager", PasswordHash = "x", Department = dept, Title = title, DeletedAt = DateTime.UtcNow };
			var employee = new User { Email = "emp@simulator.com", FirstName = "Employee", LastName = "Test", PasswordHash = "x", Department = dept, Title = title };
			ctx.Users.AddRange(activeManager, deletedManager, employee);
			await ctx.SaveChangesAsync();

			ctx.EmployeeManagers.Add(new EmployeeManager { Employee = employee, EmployeeId = employee.Id, Manager = activeManager, ManagerId = activeManager.Id });
			ctx.EmployeeManagers.Add(new EmployeeManager { Employee = employee, EmployeeId = employee.Id, Manager = deletedManager, ManagerId = deletedManager.Id });
			await ctx.SaveChangesAsync();

			var repo = new UserRepository(ctx);
			var count = await repo.GetTotalManagersCountAsync(includeDeleted: true);
			count.Should().Be(2);
		}

		[Fact]
		public async Task GetTotalManagersCountAsync_Should_Count_Manager_Only_Once_With_Multiple_Subordinates()
		{
			var ctx = CreateContext();
			var dept = new Department { Name = "OpsDept" };
			var title = new JobTitle { Name = "Tech" };
			ctx.AddRange(dept, title);
			await ctx.SaveChangesAsync();

			var manager = new User { Email = "manager@simulator.com", FirstName = "Manager", LastName = "Test", PasswordHash = "x", Department = dept, Title = title };
			var employee1 = new User { Email = "emp1@simulator.com", FirstName = "Employee1", LastName = "Test", PasswordHash = "x", Department = dept, Title = title };
			var employee2 = new User { Email = "emp2@simulator.com", FirstName = "Employee2", LastName = "Test", PasswordHash = "x", Department = dept, Title = title };
			ctx.Users.AddRange(manager, employee1, employee2);
			await ctx.SaveChangesAsync();

			ctx.EmployeeManagers.Add(new EmployeeManager { Employee = employee1, EmployeeId = employee1.Id, Manager = manager, ManagerId = manager.Id });
			ctx.EmployeeManagers.Add(new EmployeeManager { Employee = employee2, EmployeeId = employee2.Id, Manager = manager, ManagerId = manager.Id });
			await ctx.SaveChangesAsync();

			var repo = new UserRepository(ctx);
			var count = await repo.GetTotalManagersCountAsync();
			count.Should().Be(1);
		}

		[Fact]
		public async Task GetTotalManagersCountAsync_Should_Count_Multiple_Managers_With_Different_Subordinates()
		{
			var ctx = CreateContext();
			var dept = new Department { Name = "OpsDept" };
			var title = new JobTitle { Name = "Tech" };
			ctx.AddRange(dept, title);
			await ctx.SaveChangesAsync();

			var manager1 = new User { Email = "manager1@simulator.com", FirstName = "Manager1", LastName = "Test", PasswordHash = "x", Department = dept, Title = title };
			var manager2 = new User { Email = "manager2@simulator.com", FirstName = "Manager2", LastName = "Test", PasswordHash = "x", Department = dept, Title = title };
			var employee1 = new User { Email = "emp1@simulator.com", FirstName = "Employee1", LastName = "Test", PasswordHash = "x", Department = dept, Title = title };
			var employee2 = new User { Email = "emp2@simulator.com", FirstName = "Employee2", LastName = "Test", PasswordHash = "x", Department = dept, Title = title };
			ctx.Users.AddRange(manager1, manager2, employee1, employee2);
			await ctx.SaveChangesAsync();

			ctx.EmployeeManagers.Add(new EmployeeManager { Employee = employee1, EmployeeId = employee1.Id, Manager = manager1, ManagerId = manager1.Id });
			ctx.EmployeeManagers.Add(new EmployeeManager { Employee = employee2, EmployeeId = employee2.Id, Manager = manager2, ManagerId = manager2.Id });
			await ctx.SaveChangesAsync();

			var repo = new UserRepository(ctx);
			var count = await repo.GetTotalManagersCountAsync();
			count.Should().Be(2);
		}
	}
}
