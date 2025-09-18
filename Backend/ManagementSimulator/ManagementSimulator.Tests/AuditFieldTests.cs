using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ManagementSimulator.Tests
{
    public class AuditFieldTests
    {
        [Fact]
        public async Task User_Can_Be_Created_Without_Audit_Fields()
        {
            // Arrange - Create in-memory database for testing
            var options = new DbContextOptionsBuilder<MGMTSimulatorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new MGMTSimulatorDbContext(options);

            // Act - Create a user without audit service (simulating existing data)
            var testUser = new User
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1,
                DateOfEmployment = DateTime.UtcNow,
                EmploymentType = EmploymentType.FullTime,
                TotalAvailability = 1.0f,
                RemainingAvailability = 1.0f,
                Vacation = 20,
                MustChangePassword = false
            };

            // Add directly to context (simulating old data without audit)
            context.Users.Add(testUser);
            await context.SaveChangesAsync();

            // Assert
            Assert.True(testUser.Id > 0);
            Assert.Null(testUser.CreatedBy);
            Assert.Null(testUser.ModifiedBy);
            Assert.Null(testUser.DeletedBy);

            // Test reading the user
            var retrievedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
            Assert.NotNull(retrievedUser);
            Assert.Equal("Test", retrievedUser.FirstName);
        }
    }
}