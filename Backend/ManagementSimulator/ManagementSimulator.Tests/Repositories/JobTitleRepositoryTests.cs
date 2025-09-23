using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories;
using ManagementSimulator.Database.Dtos.QueryParams;
using ManagementSimulator.Tests;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace ManagementSimulator.Tests.Repositories
{
    public class JobTitleRepositoryTests : IDisposable
    {
        private readonly MGMTSimulatorDbContext _context;
        private readonly JobTitleRepository _repository;

        public JobTitleRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<MGMTSimulatorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MGMTSimulatorDbContext(options);
            _repository = new JobTitleRepository(_context, new TestAuditService());
        }

        [Fact]
        public async Task GetJobTitleByNameAsync_ShouldReturnJobTitleWhenFound()
        {
            // Arrange
            var jobTitle = new JobTitle
            {
                Name = "Software Developer"
            };

            _context.JobTitles.Add(jobTitle);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetJobTitleByNameAsync("Software Developer");

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Software Developer");
        }

        [Fact]
        public async Task GetJobTitleByNameAsync_ShouldReturnNullWhenNotFound()
        {
            // Arrange
            var jobTitle = new JobTitle
            {
                Name = "Software Developer"
            };

            _context.JobTitles.Add(jobTitle);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetJobTitleByNameAsync("Non Existent Title");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetJobTitleByNameAsync_ShouldExcludeDeletedJobTitlesByDefault()
        {
            // Arrange
            var jobTitle = new JobTitle
            {
                Name = "Software Developer",
                DeletedAt = DateTime.UtcNow
            };

            _context.JobTitles.Add(jobTitle);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetJobTitleByNameAsync("Software Developer");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetJobTitleByNameAsync_ShouldIncludeDeletedJobTitlesWhenRequested()
        {
            // Arrange
            var jobTitle = new JobTitle
            {
                Name = "Software Developer",
                DeletedAt = DateTime.UtcNow
            };

            _context.JobTitles.Add(jobTitle);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetJobTitleByNameAsync("Software Developer", includeDeleted: true);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Software Developer");
            result!.DeletedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllJobTitlesAsync_ShouldReturnAllActiveJobTitles()
        {
            // Arrange
            var jobTitle1 = new JobTitle
            {
                Name = "Software Developer"
            };
            var jobTitle2 = new JobTitle
            {
                Name = "Project Manager"
            };
            var deletedJobTitle = new JobTitle
            {
                Name = "Old Title",
                DeletedAt = DateTime.UtcNow
            };

            _context.JobTitles.AddRange(jobTitle1, jobTitle2, deletedJobTitle);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllJobTitlesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(jt => jt.Name == "Software Developer");
            result.Should().Contain(jt => jt.Name == "Project Manager");
            result.Should().NotContain(jt => jt.Name == "Old Title");
        }

        [Fact]
        public async Task GetAllJobTitlesAsync_ShouldIncludeUsers()
        {
            // Arrange
            var jobTitle = new JobTitle
            {
                Name = "Software Developer"
            };

            _context.JobTitles.Add(jobTitle);
            await _context.SaveChangesAsync();

            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = jobTitle.Id,
                DepartmentId = 1 
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllJobTitlesAsync();

            // Assert
            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Software Developer");
            result[0].Users.Should().HaveCount(1);
            result[0].Users.First().FirstName.Should().Be("John");
        }

        [Fact]
        public async Task GetJobTitleAsync_ShouldReturnJobTitleById()
        {
            // Arrange
            var jobTitle = new JobTitle
            {
                Name = "Software Developer"
            };

            _context.JobTitles.Add(jobTitle);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetJobTitleAsync(jobTitle.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(jobTitle.Id);
            result!.Name.Should().Be("Software Developer");
        }

        [Fact]
        public async Task GetJobTitleAsync_ShouldReturnNullWhenNotFound()
        {
            // Act
            var result = await _repository.GetJobTitleAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetJobTitlesAsync_ShouldReturnJobTitlesByIds()
        {
            // Arrange
            var jobTitle1 = new JobTitle
            {
                Name = "Software Developer"
            };
            var jobTitle2 = new JobTitle
            {
                Name = "Project Manager"
            };
            var jobTitle3 = new JobTitle
            {
                Name = "Designer"
            };

            _context.JobTitles.AddRange(jobTitle1, jobTitle2, jobTitle3);
            await _context.SaveChangesAsync();

            var requestedIds = new List<int> { jobTitle1.Id, jobTitle3.Id };

            // Act
            var result = await _repository.GetJobTitlesAsync(requestedIds);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(jt => jt.Id == jobTitle1.Id);
            result.Should().Contain(jt => jt.Id == jobTitle3.Id);
            result.Should().NotContain(jt => jt.Id == jobTitle2.Id);
        }

        [Fact]
        public async Task GetAllJobTitlesFilteredAsync_ShouldReturnAllActiveJobTitles()
        {
            // Arrange
            var jobTitle1 = new JobTitle
            {
                Name = "Software Developer"
            };
            var jobTitle2 = new JobTitle
            {
                Name = "Project Manager"
            };
            var deletedJobTitle = new JobTitle
            {
                Name = "Old Title",
                DeletedAt = DateTime.UtcNow
            };

            _context.JobTitles.AddRange(jobTitle1, jobTitle2, deletedJobTitle);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams();

            // Act
            var result = await _repository.GetAllJobTitlesFilteredAsync(null, parameters);

            // Assert
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.Data.Should().Contain(jt => jt.Name == "Software Developer");
            result.Data.Should().Contain(jt => jt.Name == "Project Manager");
            result.Data.Should().NotContain(jt => jt.Name == "Old Title");
        }

        [Fact]
        public async Task GetAllJobTitlesFilteredAsync_ShouldFilterByName()
        {
            // Arrange
            var jobTitle1 = new JobTitle
            {
                Name = "Software Developer"
            };
            var jobTitle2 = new JobTitle
            {
                Name = "Project Manager"
            };

            _context.JobTitles.AddRange(jobTitle1, jobTitle2);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams();

            // Act
            var result = await _repository.GetAllJobTitlesFilteredAsync("Software", parameters);

            // Assert
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Data[0].Name.Should().Be("Software Developer");
        }

        [Fact]
        public async Task GetAllJobTitlesFilteredAsync_ShouldApplyPagination()
        {
            // Arrange
            var jobTitles = new List<JobTitle>();
            for (int i = 1; i <= 5; i++)
            {
                jobTitles.Add(new JobTitle
                {
                    Name = $"Job Title {i}"
                });
            }

            _context.JobTitles.AddRange(jobTitles);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams
            {
                Page = 2,
                PageSize = 2
            };

            // Act
            var result = await _repository.GetAllJobTitlesFilteredAsync(null, parameters);

            // Assert
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(5);
            result.Data[0].Name.Should().Be("Job Title 3");
            result.Data[1].Name.Should().Be("Job Title 4");
        }

        [Fact]
        public async Task GetAllJobTitlesFilteredAsync_ShouldApplySortingByName()
        {
            // Arrange
            var jobTitle1 = new JobTitle
            {
                Name = "Z Job Title"
            };
            var jobTitle2 = new JobTitle
            {
                Name = "A Job Title"
            };

            _context.JobTitles.AddRange(jobTitle1, jobTitle2);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams
            {
                SortBy = "jobTitleName"
            };

            // Act
            var result = await _repository.GetAllJobTitlesFilteredAsync(null, parameters);

            // Assert
            result.Data.Should().HaveCount(2);
            result.Data[0].Name.Should().Be("A Job Title");
            result.Data[1].Name.Should().Be("Z Job Title");
        }

        [Fact]
        public async Task GetAllJobTitlesFilteredAsync_ShouldDefaultSortById()
        {
            // Arrange
            var jobTitle1 = new JobTitle
            {
                Name = "Z Job Title"
            };
            var jobTitle2 = new JobTitle
            {
                Name = "A Job Title"
            };

            _context.JobTitles.AddRange(jobTitle1, jobTitle2);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams();

            // Act
            var result = await _repository.GetAllJobTitlesFilteredAsync(null, parameters);

            // Assert
            result.Data.Should().HaveCount(2);
            result.Data[0].Name.Should().Be("Z Job Title");
            result.Data[1].Name.Should().Be("A Job Title");
        }

        [Fact]
        public async Task GetAllInactiveJobTitlesFilteredAsync_ShouldReturnOnlyDeletedJobTitles()
        {
            // Arrange
            var activeJobTitle = new JobTitle
            {
                Name = "Active Job Title"
            };
            var deletedJobTitle = new JobTitle
            {
                Name = "Deleted Job Title",
                DeletedAt = DateTime.UtcNow
            };

            _context.JobTitles.AddRange(activeJobTitle, deletedJobTitle);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams();

            // Act
            var result = await _repository.GetAllInactiveJobTitlesFilteredAsync(null, parameters);

            // Assert
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Data[0].Name.Should().Be("Deleted Job Title");
            result.Data[0].DeletedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllInactiveJobTitlesFilteredAsync_ShouldFilterDeletedJobTitlesByName()
        {
            // Arrange
            var deletedJobTitle1 = new JobTitle
            {
                Name = "Deleted Software Developer",
                DeletedAt = DateTime.UtcNow
            };
            var deletedJobTitle2 = new JobTitle
            {
                Name = "Deleted Project Manager",
                DeletedAt = DateTime.UtcNow
            };

            _context.JobTitles.AddRange(deletedJobTitle1, deletedJobTitle2);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams();

            // Act
            var result = await _repository.GetAllInactiveJobTitlesFilteredAsync("Software", parameters);

            // Assert
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Data[0].Name.Should().Be("Deleted Software Developer");
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
