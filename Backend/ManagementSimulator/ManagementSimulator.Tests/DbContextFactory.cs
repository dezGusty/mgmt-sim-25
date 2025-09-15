using System;
using Microsoft.EntityFrameworkCore;
using ManagementSimulator.Database.Context;

namespace ManagementSimulator.Tests
{
	public static class DbContextFactory
	{
		public static MGMTSimulatorDbContext CreateInMemoryContext(string? databaseName = null)
		{
			var options = new DbContextOptionsBuilder<MGMTSimulatorDbContext>()
				.UseInMemoryDatabase(databaseName ?? $"TestsDb_{Guid.NewGuid()}")
				.Options;

			return new MGMTSimulatorDbContext(options);
		}
	}
}
