using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ManagementSimulator.Infrastructure.Config;
using System.IO;

namespace ManagementSimulator.Database.Context
{
    public class MGMTSimulatorDbContextFactory : IDesignTimeDbContextFactory<MGMTSimulatorDbContext>
    {
        public MGMTSimulatorDbContext CreateDbContext(string[] args)
        {
            var current = Directory.GetCurrentDirectory();
            var candidateBases = new[]
            {
                current,
                Path.GetFullPath(Path.Combine(current, "..", "ManagementSimulator"))
            };

            string basePath = candidateBases.FirstOrDefault(path =>
                File.Exists(Path.Combine(path, "appsettings.Development.json")) ||
                File.Exists(Path.Combine(path, "appsettings.json")))
                ?? current;

            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.json", optional: true); // fallback option

            var configuration = builder.Build();
            AppConfig.Init(configuration);

            var optionsBuilder = new DbContextOptionsBuilder<MGMTSimulatorDbContext>();
            optionsBuilder.UseSqlite(AppConfig.ConnectionStrings?.MGMTSimulatorDb);

            if (AppConfig.ConsoleLogQueries)
            {
                optionsBuilder.LogTo(Console.WriteLine);
            }

            return new MGMTSimulatorDbContext(optionsBuilder.Options);
        }
    }
}
