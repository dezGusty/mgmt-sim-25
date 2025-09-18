using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Infrastructure.Config
{
    public static class AppConfig
    {
        public static bool ConsoleLogQueries { get; set; } = true;
        public static ConnectionStrings? ConnectionStrings { get; set; }
        public static WeekendConfiguration? WeekendSettings { get; set; }
        private static IConfiguration? _configuration;

        public static void Init(IConfiguration configuration)
        {
            _configuration = configuration;
            Initialize(configuration);
        }

        public static void Reinitialize()
        {
            if (_configuration != null)
            {
                Initialize(_configuration);
            }
        }

        public static void Initialize(IConfiguration configuration)
        {
            ConnectionStrings = configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>();
            WeekendSettings = configuration.GetSection("WeekendSettings").Get<WeekendConfiguration>();
            
            if (WeekendSettings == null)
            {
                WeekendSettings = new WeekendConfiguration
                {
                    WeekendDays = new List<string> { "Saturday", "Sunday" },
                    WeekendDaysCount = 2
                };
            }
            else if (!WeekendSettings.IsValid())
            {
                Console.WriteLine($"Invalid WeekendSettings configuration detected. WeekendDays: [{string.Join(", ", WeekendSettings.WeekendDays)}], Count: {WeekendSettings.WeekendDaysCount}");
            }
        }
    }
}
