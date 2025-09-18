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
    public class ProjectRepositoryTests : IDisposable
    {
        private readonly MGMTSimulatorDbContext _context;
        private readonly ProjectRepository _repository;

        public ProjectRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<MGMTSimulatorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MGMTSimulatorDbContext(options);
            _repository = new ProjectRepository(_context, new TestAuditService());
        }

        [Fact]
        public async Task GetProjectByIdAsync_ShouldReturnProjectWhenFound()
        {
            // Arrange
            var project = new Project
            {
                Name = "Test Project",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 5.0f,
                IsActive = true
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetProjectByIdAsync(project.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(project.Id);
            result!.Name.Should().Be("Test Project");
            result!.BudgetedFTEs.Should().Be(5.0f);
            result!.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task GetProjectByIdAsync_ShouldReturnNullWhenNotFound()
        {
            // Act
            var result = await _repository.GetProjectByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProjectByIdAsync_ShouldExcludeDeletedProjectsByDefault()
        {
            // Arrange
            var project = new Project
            {
                Name = "Test Project",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 5.0f,
                IsActive = true,
                DeletedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetProjectByIdAsync(project.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProjectByIdAsync_ShouldIncludeDeletedProjectsWhenRequested()
        {
            // Arrange
            var project = new Project
            {
                Name = "Test Project",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 5.0f,
                IsActive = true,
                DeletedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetProjectByIdAsync(project.Id, includeDeleted: true);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(project.Id);
            result!.DeletedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task GetProjectByNameAsync_ShouldReturnProjectWhenFound()
        {
            // Arrange
            var project = new Project
            {
                Name = "Unique Project Name",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 3.0f,
                IsActive = true
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetProjectByNameAsync("Unique Project Name");

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Unique Project Name");
        }

        [Fact]
        public async Task GetProjectByNameAsync_ShouldReturnNullWhenNotFound()
        {
            // Arrange
            var project = new Project
            {
                Name = "Test Project",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 3.0f,
                IsActive = true
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetProjectByNameAsync("Non Existent Project");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProjectWithUsersAsync_ShouldReturnProjectWithUsers()
        {
            // Arrange
            var project = new Project
            {
                Name = "Test Project",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 2.0f,
                IsActive = true
            };

            var jobTitle = new JobTitle
            {
                Name = "Software Developer"
            };
            var department = new Department
            {
                Name = "IT Department",
                Description = "Information Technology"
            };

            _context.JobTitles.Add(jobTitle);
            _context.Departments.Add(department);
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Add user to project
            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = jobTitle.Id,
                DepartmentId = department.Id
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userProject = new UserProject
            {
                UserId = user.Id,
                ProjectId = project.Id,
                TimePercentagePerProject = 0.50f
            };

            _context.UserProjects.Add(userProject);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetProjectWithUsersAsync(project.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Test Project");
            result!.UserProjects.Should().HaveCount(1);
            result!.UserProjects.First().User.FirstName.Should().Be("John");
        }

        [Fact]
        public async Task GetAllProjectsAsync_ShouldReturnProjectsByIds()
        {
            // Arrange
            var project1 = new Project
            {
                Name = "Project 1",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 6, 30),
                BudgetedFTEs = 2.0f,
                IsActive = true
            };
            var project2 = new Project
            {
                Name = "Project 2",
                StartDate = new DateTime(2024, 7, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 3.0f,
                IsActive = true
            };
            var project3 = new Project
            {
                Name = "Project 3",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 1.0f,
                IsActive = false
            };

            _context.Projects.AddRange(project1, project2, project3);
            await _context.SaveChangesAsync();

            var requestedIds = new List<int> { project1.Id, project3.Id };

            // Act
            var result = await _repository.GetAllProjectsAsync(requestedIds);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(p => p.Id == project1.Id);
            result.Should().Contain(p => p.Id == project3.Id);
            result.Should().NotContain(p => p.Id == project2.Id);
        }

        [Fact]
        public async Task GetAllProjectsFilteredAsync_ShouldReturnAllActiveProjects()
        {
            // Arrange
            var project1 = new Project
            {
                Name = "Active Project 1",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 2.0f,
                IsActive = true
            };
            var project2 = new Project
            {
                Name = "Active Project 2",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 3.0f,
                IsActive = true
            };
            var deletedProject = new Project
            {
                Name = "Deleted Project",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 1.0f,
                IsActive = true,
                DeletedAt = DateTime.UtcNow
            };

            _context.Projects.AddRange(project1, project2, deletedProject);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams();

            // Act
            var result = await _repository.GetAllProjectsFilteredAsync(null, null, null, null, null, null, parameters);

            // Assert
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.Data.Should().Contain(p => p.Name == "Active Project 1");
            result.Data.Should().Contain(p => p.Name == "Active Project 2");
            result.Data.Should().NotContain(p => p.Name == "Deleted Project");
        }

        [Fact]
        public async Task GetAllProjectsFilteredAsync_ShouldFilterByName()
        {
            // Arrange
            var project1 = new Project
            {
                Name = "Software Development Project",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 2.0f,
                IsActive = true
            };
            var project2 = new Project
            {
                Name = "Marketing Campaign",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 3.0f,
                IsActive = true
            };

            _context.Projects.AddRange(project1, project2);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams();

            // Act
            var result = await _repository.GetAllProjectsFilteredAsync("Software", null, null, null, null, null, parameters);

            // Assert
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Data[0].Name.Should().Be("Software Development Project");
        }

        [Fact]
        public async Task GetAllProjectsFilteredAsync_ShouldFilterByIsActive()
        {
            // Arrange
            var activeProject = new Project
            {
                Name = "Active Project",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 2.0f,
                IsActive = true
            };
            var inactiveProject = new Project
            {
                Name = "Inactive Project",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 3.0f,
                IsActive = false
            };

            _context.Projects.AddRange(activeProject, inactiveProject);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams();

            // Act
            var result = await _repository.GetAllProjectsFilteredAsync(null, true, null, null, null, null, parameters);

            // Assert
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Data[0].Name.Should().Be("Active Project");
            result.Data[0].IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task GetAllProjectsFilteredAsync_ShouldFilterByDateRange()
        {
            // Arrange
            var project1 = new Project
            {
                Name = "Early Project",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 6, 30),
                BudgetedFTEs = 2.0f,
                IsActive = true
            };
            var project2 = new Project
            {
                Name = "Late Project",
                StartDate = new DateTime(2024, 7, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 3.0f,
                IsActive = true
            };

            _context.Projects.AddRange(project1, project2);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams();
            var startDateFrom = new DateTime(2024, 6, 1);

            // Act
            var result = await _repository.GetAllProjectsFilteredAsync(null, null, startDateFrom, null, null, null, parameters);

            // Assert
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.Data[0].Name.Should().Be("Late Project");
        }

        [Fact]
        public async Task GetAllProjectsFilteredAsync_ShouldApplyPagination()
        {
            // Arrange
            var projects = new List<Project>();
            for (int i = 1; i <= 5; i++)
            {
                projects.Add(new Project
                {
                    Name = $"Project {i}",
                    StartDate = new DateTime(2024, 1, 1),
                    EndDate = new DateTime(2024, 12, 31),
                    BudgetedFTEs = i * 0.5f,
                    IsActive = true
                });
            }

            _context.Projects.AddRange(projects);
            await _context.SaveChangesAsync();

            var parameters = new QueryParams
            {
                Page = 2,
                PageSize = 2
            };

            // Act
            var result = await _repository.GetAllProjectsFilteredAsync(null, null, null, null, null, null, parameters);

            // Assert
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(5);
            result.Data[0].Name.Should().Be("Project 3");
            result.Data[1].Name.Should().Be("Project 4");
        }

        [Fact]
        public async Task GetUserProjectAsync_ShouldReturnUserProjectWhenFound()
        {
            // Arrange
            var project = new Project
            {
                Name = "Test Project",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 2.0f,
                IsActive = true
            };

            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Projects.Add(project);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userProject = new UserProject
            {
                UserId = user.Id,
                ProjectId = project.Id,
                TimePercentagePerProject = 75.0f
            };

            _context.UserProjects.Add(userProject);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserProjectAsync(user.Id, project.Id);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(user.Id);
            result!.ProjectId.Should().Be(project.Id);
            result!.TimePercentagePerProject.Should().Be(75.0f);
        }

        [Fact]
        public async Task GetUserProjectAsync_ShouldReturnNullWhenNotFound()
        {
            // Act
            var result = await _repository.GetUserProjectAsync(999, 999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProjectUsersAsync_ShouldReturnUsersAssignedToProject()
        {
            // Arrange
            var project = new Project
            {
                Name = "Test Project",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 2.0f,
                IsActive = true
            };

            var user1 = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };
            var user2 = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Projects.Add(project);
            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var userProject1 = new UserProject
            {
                UserId = user1.Id,
                ProjectId = project.Id,
                TimePercentagePerProject = 50.0f
            };
            var userProject2 = new UserProject
            {
                UserId = user2.Id,
                ProjectId = project.Id,
                TimePercentagePerProject = 30.0f
            };

            _context.UserProjects.AddRange(userProject1, userProject2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetProjectUsersAsync(project.Id);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(up => up.User.FirstName == "John");
            result.Should().Contain(up => up.User.FirstName == "Jane");
        }

        [Fact]
        public async Task GetUserProjectsByUserIdAsync_ShouldReturnProjectsForUser()
        {
            // Arrange
            var project1 = new Project
            {
                Name = "Project 1",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 6, 30),
                BudgetedFTEs = 2.0f,
                IsActive = true
            };
            var project2 = new Project
            {
                Name = "Project 2",
                StartDate = new DateTime(2024, 7, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 3.0f,
                IsActive = true
            };

            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Projects.AddRange(project1, project2);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userProject1 = new UserProject
            {
                UserId = user.Id,
                ProjectId = project1.Id,
                TimePercentagePerProject = 60.0f
            };
            var userProject2 = new UserProject
            {
                UserId = user.Id,
                ProjectId = project2.Id,
                TimePercentagePerProject = 40.0f
            };

            _context.UserProjects.AddRange(userProject1, userProject2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserProjectsByUserIdAsync(user.Id);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(up => up.Project.Name == "Project 1");
            result.Should().Contain(up => up.Project.Name == "Project 2");
        }

        [Fact]
        public async Task AddUserToProjectAsync_ShouldAddUserToProject()
        {
            // Arrange
            var project = new Project
            {
                Name = "Test Project",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 2.0f,
                IsActive = true
            };

            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Projects.Add(project);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userProject = new UserProject
            {
                UserId = user.Id,
                ProjectId = project.Id,
                TimePercentagePerProject = 100.0f
            };

            // Act
            var result = await _repository.AddUserToProjectAsync(userProject);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(user.Id);
            result.ProjectId.Should().Be(project.Id);
            result.TimePercentagePerProject.Should().Be(100.0f);

            // Verify it was saved to database
            var savedUserProject = await _repository.GetUserProjectAsync(user.Id, project.Id);
            savedUserProject.Should().NotBeNull();
        }

        [Fact]
        public async Task RemoveUserFromProjectAsync_ShouldRemoveUserFromProject()
        {
            // Arrange
            var project = new Project
            {
                Name = "Test Project",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 2.0f,
                IsActive = true
            };

            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Projects.Add(project);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userProject = new UserProject
            {
                UserId = user.Id,
                ProjectId = project.Id,
                TimePercentagePerProject = 100.0f
            };

            _context.UserProjects.Add(userProject);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.RemoveUserFromProjectAsync(user.Id, project.Id);

            // Assert
            result.Should().BeTrue();

            // Verify it was removed from database
            var removedUserProject = await _repository.GetUserProjectAsync(user.Id, project.Id);
            removedUserProject.Should().BeNull();
        }

        [Fact]
        public async Task RemoveUserFromProjectAsync_ShouldReturnFalseWhenUserProjectNotFound()
        {
            // Act
            var result = await _repository.RemoveUserFromProjectAsync(999, 999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateUserProjectAssignmentAsync_ShouldUpdateTimePercentage()
        {
            // Arrange
            var project = new Project
            {
                Name = "Test Project",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                BudgetedFTEs = 2.0f,
                IsActive = true
            };

            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PasswordHash = "hash",
                JobTitleId = 1,
                DepartmentId = 1
            };

            _context.Projects.Add(project);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userProject = new UserProject
            {
                UserId = user.Id,
                ProjectId = project.Id,
                TimePercentagePerProject = 50.0f
            };

            _context.UserProjects.Add(userProject);
            await _context.SaveChangesAsync();

            // Update the time percentage
            userProject.TimePercentagePerProject = 75.0f;

            // Act
            var result = await _repository.UpdateUserProjectAssignmentAsync(userProject);

            // Assert
            result.Should().BeTrue();

            // Verify the update
            var updatedUserProject = await _repository.GetUserProjectAsync(user.Id, project.Id);
            updatedUserProject.Should().NotBeNull();
            updatedUserProject!.TimePercentagePerProject.Should().Be(75.0f);
        }

        [Fact]
        public async Task UpdateUserProjectAssignmentAsync_ShouldReturnFalseWhenUserProjectNotFound()
        {
            // Arrange
            var userProject = new UserProject
            {
                UserId = 999,
                ProjectId = 999,
                TimePercentagePerProject = 75.0f
            };

            // Act
            var result = await _repository.UpdateUserProjectAssignmentAsync(userProject);

            // Assert
            result.Should().BeFalse();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
