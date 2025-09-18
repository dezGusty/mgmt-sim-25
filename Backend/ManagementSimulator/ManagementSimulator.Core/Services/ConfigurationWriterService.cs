using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Infrastructure.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services
{
    public class ConfigurationWriterService : IConfigurationWriterService
    {
        private readonly IHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigurationWriterService> _logger;

        public ConfigurationWriterService(
            IHostEnvironment environment,
            IConfiguration configuration,
            ILogger<ConfigurationWriterService> logger)
        {
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> UpdateWeekendSettingsAsync(WeekendConfiguration weekendConfiguration)
        {
            try
            {
                // Only update the base configuration file
                // Weekend settings will be inherited by all environments
                var result = await UpdateAppSettingsFileAsync("appsettings.json", weekendConfiguration);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating weekend settings in configuration files");
                return false;
            }
        }

        private async Task<bool> UpdateAppSettingsFileAsync(string fileName, WeekendConfiguration weekendConfiguration)
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
                
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("Configuration file {FileName} not found at {FilePath}", fileName, filePath);
                    return false;
                }

                var json = await File.ReadAllTextAsync(filePath);
                var jsonObject = JObject.Parse(json);

                var weekendSettings = new JObject
                {
                    ["WeekendDays"] = new JArray(weekendConfiguration.WeekendDays),
                    ["WeekendDaysCount"] = weekendConfiguration.WeekendDaysCount
                };

                jsonObject["WeekendSettings"] = weekendSettings;

                var formattedJson = jsonObject.ToString(Formatting.Indented);
                await File.WriteAllTextAsync(filePath, formattedJson);

                _logger.LogInformation("Successfully updated weekend settings in {FileName}", fileName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {FileName}", fileName);
                return false;
            }
        }

        public Task ReloadConfigurationAsync()
        {
            try
            {
                var configurationBuilder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddJsonFile($"appsettings.{_environment.EnvironmentName}.json", optional: true, reloadOnChange: false);

                var newConfiguration = configurationBuilder.Build();
                
                AppConfig.Initialize(newConfiguration);
                
                _logger.LogInformation("Configuration reloaded successfully from files");
                
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading configuration");
                throw;
            }
        }
    }
}
