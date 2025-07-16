using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ManagementSimulator.Infrastructure.Config;

namespace ManagementSimulator.Database.Context
{
    public class MGMTSimulatorDbContextFactory : IDesignTimeDbContextFactory<MGMTSimulatorDbContext>
    {
        public MGMTSimulatorDbContext CreateDbContext(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.json", optional: true); // fallback option

            var configuration = builder.Build();
            AppConfig.Init(configuration);

            var optionsBuilder = new DbContextOptionsBuilder<MGMTSimulatorDbContext>();
            optionsBuilder.UseSqlServer(AppConfig.ConnectionStrings?.MGMTSimulatorDb);

            if (AppConfig.ConsoleLogQueries)
            {
                optionsBuilder.LogTo(Console.WriteLine);
            }

            return new MGMTSimulatorDbContext(optionsBuilder.Options);
        }
    }
}
