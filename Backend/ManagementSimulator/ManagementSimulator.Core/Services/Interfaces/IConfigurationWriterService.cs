using ManagementSimulator.Infrastructure.Config;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface IConfigurationWriterService
    {
        Task<bool> UpdateWeekendSettingsAsync(WeekendConfiguration weekendConfiguration);
        Task ReloadConfigurationAsync();
    }
}
