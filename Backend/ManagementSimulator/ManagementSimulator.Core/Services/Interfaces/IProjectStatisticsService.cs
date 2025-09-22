using ManagementSimulator.Core.Dtos.Requests.ProjectStatistics;
using ManagementSimulator.Core.Dtos.Responses.ProjectStatistics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManagementSimulator.Core.Services.Interfaces
{
    public interface IProjectStatisticsService
    {
        Task<ProjectStatisticsResponseDto> GetProjectStatisticsAsync(ProjectStatisticsRequestDto request);
        Task<ProjectStatisticsOverviewDto> GetProjectStatisticsOverviewAsync(ProjectStatisticsOverviewRequestDto request);
        Task<StatisticsChartDataDto> GetProjectAllocationChartDataAsync(ProjectAllocationChartRequestDto request);
        Task<StatisticsChartDataDto> GetProjectBudgetChartDataAsync(ProjectBudgetChartRequestDto request);
        Task<List<AllocationEventDto>> GetProjectAllocationEventsAsync(int projectId, DateTime startDate, DateTime endDate);
        Task<FiscalYearDto> GetCurrentFiscalYearAsync();
        Task<FiscalYearDto> GetFiscalYearAsync(int year);
        Task<List<string>> GetAvailableMonthsForProjectAsync(int projectId, int fiscalYear);
    }
}