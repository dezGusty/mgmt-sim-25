using System;
using System.Collections.Generic;

namespace ManagementSimulator.Core.Dtos.Responses.ProjectStatistics
{
    public class ProjectStatisticsResponseDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public FiscalYearDto FiscalYear { get; set; } = new FiscalYearDto();
        public List<MonthlyAllocationDataDto> MonthlyAllocationData { get; set; } = new List<MonthlyAllocationDataDto>();
        public List<EmployeeActivityDataDto> EmployeeActivity { get; set; } = new List<EmployeeActivityDataDto>();
        public List<BudgetUtilizationDto> BudgetUtilization { get; set; } = new List<BudgetUtilizationDto>();
        public List<ProjectMilestoneDto> Milestones { get; set; } = new List<ProjectMilestoneDto>();
        public ProjectActivitySummaryDto Summary { get; set; } = new ProjectActivitySummaryDto();
        public DateTime LastUpdated { get; set; }
    }

    public class ProjectStatisticsOverviewDto
    {
        public FiscalYearDto FiscalYear { get; set; } = new FiscalYearDto();
        public string? SelectedMonth { get; set; } // YYYY-MM format
        public List<string> AvailableMonths { get; set; } = new List<string>(); // All months in fiscal year with data
        public int TotalProjects { get; set; }
        public float TotalBudgetedFTEs { get; set; }
        public float TotalAllocatedFTEs { get; set; }
        public float AverageUtilization { get; set; }
    }

    public class ChartDataPointDto
    {
        public string Date { get; set; } = string.Empty;
        public float Value { get; set; }
        public string? Label { get; set; }
    }

    public class StatisticsChartDataDto
    {
        public string Title { get; set; } = string.Empty;
        public List<ChartDataPointDto> Data { get; set; } = new List<ChartDataPointDto>();
        public string Type { get; set; } = string.Empty; // 'line', 'bar', 'area'
        public string Color { get; set; } = string.Empty;
        public List<ChartDataPointDto>? PreviousPeriodData { get; set; }
    }

    public class AllocationEventDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeEmail { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // 'assigned', 'removed', 'percentage_changed'
        public float? OldPercentage { get; set; }
        public float? NewPercentage { get; set; }
        public DateTime Timestamp { get; set; }
        public int? AuditLogId { get; set; }
    }
}