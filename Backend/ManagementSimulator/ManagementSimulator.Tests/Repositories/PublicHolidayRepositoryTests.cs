using ManagementSimulator.Database.Context;
using ManagementSimulator.Database.Entities;
using ManagementSimulator.Database.Repositories;
using ManagementSimulator.Tests;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace ManagementSimulator.Tests.Repositories
{
    public class PublicHolidayRepositoryTests : IDisposable
    {
        private readonly MGMTSimulatorDbContext _context;
        private readonly PublicHolidayRepository _repository;

        public PublicHolidayRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<MGMTSimulatorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MGMTSimulatorDbContext(options);
            _repository = new PublicHolidayRepository(_context, new TestAuditService());
        }

        [Fact]
        public async Task GetHolidaysByYearAsync_ShouldReturnHolidaysForSpecificYear()
        {
            // Arrange
            var year = 2024;
            var holiday1 = new PublicHoliday
            {
                Name = "New Year",
                Date = new DateTime(year, 1, 1),
                IsRecurring = true
            };
            var holiday2 = new PublicHoliday
            {
                Name = "Christmas",
                Date = new DateTime(year, 12, 25),
                IsRecurring = true
            };
            var holiday3 = new PublicHoliday
            {
                Name = "Old Holiday",
                Date = new DateTime(2023, 1, 1),
                IsRecurring = false
            };

            _context.PublicHolidays.AddRange(holiday1, holiday2, holiday3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetHolidaysByYearAsync(year);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(h => h.Name == "New Year");
            result.Should().Contain(h => h.Name == "Christmas");
            result.Should().NotContain(h => h.Name == "Old Holiday");
        }

        [Fact]
        public async Task GetHolidaysByYearAsync_ShouldReturnEmptyListWhenNoHolidaysExist()
        {
            // Arrange
            var year = 2024;

            // Act
            var result = await _repository.GetHolidaysByYearAsync(year);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetHolidaysByYearAsync_ShouldOrderByDate()
        {
            // Arrange
            var year = 2024;
            var holiday1 = new PublicHoliday
            {
                Name = "Christmas",
                Date = new DateTime(year, 12, 25),
                IsRecurring = true
            };
            var holiday2 = new PublicHoliday
            {
                Name = "New Year",
                Date = new DateTime(year, 1, 1),
                IsRecurring = true
            };

            _context.PublicHolidays.AddRange(holiday1, holiday2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetHolidaysByYearAsync(year);

            // Assert
            result.Should().HaveCount(2);
            result[0].Name.Should().Be("New Year");
            result[1].Name.Should().Be("Christmas");
        }

        [Fact]
        public async Task GetHolidayByNameAndDateAsync_ShouldReturnHolidayWhenFound()
        {
            // Arrange
            var holiday = new PublicHoliday
            {
                Name = "Test Holiday",
                Date = new DateTime(2024, 6, 15),
                IsRecurring = false
            };

            _context.PublicHolidays.Add(holiday);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetHolidayByNameAndDateAsync("Test Holiday", new DateTime(2024, 6, 15));

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Test Holiday");
            result!.Date.Should().Be(new DateTime(2024, 6, 15));
        }

        [Fact]
        public async Task GetHolidayByNameAndDateAsync_ShouldBeCaseInsensitive()
        {
            // Arrange
            var holiday = new PublicHoliday
            {
                Name = "Test Holiday",
                Date = new DateTime(2024, 6, 15),
                IsRecurring = false
            };

            _context.PublicHolidays.Add(holiday);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetHolidayByNameAndDateAsync("test holiday", new DateTime(2024, 6, 15));

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Test Holiday");
        }

        [Fact]
        public async Task GetHolidayByNameAndDateAsync_ShouldReturnNullWhenNotFound()
        {
            // Arrange
            var holiday = new PublicHoliday
            {
                Name = "Test Holiday",
                Date = new DateTime(2024, 6, 15),
                IsRecurring = false
            };

            _context.PublicHolidays.Add(holiday);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetHolidayByNameAndDateAsync("Non Existent", new DateTime(2024, 6, 15));

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetHolidaysInRangeAsync_ShouldReturnHolidaysInDateRange()
        {
            // Arrange
            var startDate = new DateTime(2024, 6, 1);
            var endDate = new DateTime(2024, 6, 30);

            var holiday1 = new PublicHoliday
            {
                Name = "June Holiday 1",
                Date = new DateTime(2024, 6, 15),
                IsRecurring = false
            };
            var holiday2 = new PublicHoliday
            {
                Name = "June Holiday 2",
                Date = new DateTime(2024, 6, 20),
                IsRecurring = false
            };
            var holiday3 = new PublicHoliday
            {
                Name = "May Holiday",
                Date = new DateTime(2024, 5, 31),
                IsRecurring = false
            };
            var holiday4 = new PublicHoliday
            {
                Name = "July Holiday",
                Date = new DateTime(2024, 7, 1),
                IsRecurring = false
            };

            _context.PublicHolidays.AddRange(holiday1, holiday2, holiday3, holiday4);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetHolidaysInRangeAsync(startDate, endDate);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(h => h.Name == "June Holiday 1");
            result.Should().Contain(h => h.Name == "June Holiday 2");
            result.Should().NotContain(h => h.Name == "May Holiday");
            result.Should().NotContain(h => h.Name == "July Holiday");
        }

        [Fact]
        public async Task GetHolidaysInRangeAsync_ShouldOrderByDate()
        {
            // Arrange
            var startDate = new DateTime(2024, 6, 1);
            var endDate = new DateTime(2024, 6, 30);

            var holiday1 = new PublicHoliday
            {
                Name = "June Holiday 2",
                Date = new DateTime(2024, 6, 20),
                IsRecurring = false
            };
            var holiday2 = new PublicHoliday
            {
                Name = "June Holiday 1",
                Date = new DateTime(2024, 6, 15),
                IsRecurring = false
            };

            _context.PublicHolidays.AddRange(holiday1, holiday2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetHolidaysInRangeAsync(startDate, endDate);

            // Assert
            result.Should().HaveCount(2);
            result[0].Name.Should().Be("June Holiday 1");
            result[1].Name.Should().Be("June Holiday 2");
        }

        [Fact]
        public async Task GetHolidaysInRangeAsync_ShouldReturnEmptyListWhenNoHolidaysInRange()
        {
            // Arrange
            var startDate = new DateTime(2024, 6, 1);
            var endDate = new DateTime(2024, 6, 30);

            var holiday = new PublicHoliday
            {
                Name = "May Holiday",
                Date = new DateTime(2024, 5, 31),
                IsRecurring = false
            };

            _context.PublicHolidays.Add(holiday);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetHolidaysInRangeAsync(startDate, endDate);

            // Assert
            result.Should().BeEmpty();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
