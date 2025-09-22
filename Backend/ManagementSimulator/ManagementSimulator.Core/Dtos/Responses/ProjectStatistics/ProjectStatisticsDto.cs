using System;
using System.Collections.Generic;

namespace ManagementSimulator.Core.Dtos.Responses.ProjectStatistics
{
    public class MonthlyAllocationDataDto
    {
        public string Month { get; set; } = string.Empty; // YYYY-MM format
        public int Allocations { get; set; }
        public int Deallocations { get; set; }
        public int TotalEmployees { get; set; }
        public float TotalFTEs { get; set; }
        public List<DailyAllocationDataDto> DailyData { get; set; } = new List<DailyAllocationDataDto>();
        public MonthTrendComparisonDto? TrendComparison { get; set; }
    }

    public class DailyAllocationDataDto
    {
        public string Date { get; set; } = string.Empty; // YYYY-MM-DD format
        public int TotalEmployees { get; set; }
        public float TotalFTEs { get; set; }
        public int CumulativeAllocations { get; set; }
        public int CumulativeDeallocations { get; set; }
    }

    public class BudgetUtilizationDto
    {
        public string Month { get; set; } = string.Empty;
        public float BudgetedFTEs { get; set; }
        public float ActualFTEs { get; set; }
        public float UtilizationPercentage { get; set; }
        public float Variance { get; set; }
        public List<DailyBudgetDataDto> DailyData { get; set; } = new List<DailyBudgetDataDto>();
        public MonthTrendComparisonDto? TrendComparison { get; set; }
    }

    public class DailyBudgetDataDto
    {
        public string Date { get; set; } = string.Empty; // YYYY-MM-DD format
        public float BudgetedFTEs { get; set; }
        public float ActualFTEs { get; set; }
        public float UtilizationPercentage { get; set; }
        public float Variance { get; set; }
    }

    public class MonthTrendComparisonDto
    {
        public string PreviousMonth { get; set; } = string.Empty;
        public string CurrentMonth { get; set; } = string.Empty;
        public float AllocationsChange { get; set; } // percentage change
        public float BudgetUtilizationChange { get; set; } // percentage change
        public int EmployeeCountChange { get; set; } // absolute change
        public float FtesChange { get; set; } // absolute change
        public string Trend { get; set; } = string.Empty; // "improving", "declining", "stable"
    }

    public class FiscalYearDto
    {
        public DateTime StartDate { get; set; } // October 1st
        public DateTime EndDate { get; set; } // September 30th
        public int Year { get; set; } // The year when fiscal year starts
        public string Label { get; set; } = string.Empty; // "FY 2024-2025"
        public int DaysRemaining { get; set; } // Days until next fiscal year starts
        public bool IsCurrentFiscalYear { get; set; } // Whether this is the current active fiscal year
    }

    public class ProjectActivitySummaryDto
    {
        public int TotalEmployeesEver { get; set; }
        public int CurrentEmployees { get; set; }
        public float AverageAllocationPercentage { get; set; }
        public int PeakEmployeeCount { get; set; }
        public string PeakEmployeeMonth { get; set; } = string.Empty;
        public int TotalAllocationEvents { get; set; }
        public int TotalDeallocationEvents { get; set; }
        public int ProjectDuration { get; set; } // in months
    }

    public class ProjectMilestoneDto
    {
        public string Date { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // 'project_start', 'project_end', 'budget_change', 'major_assignment'
        public string Description { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty; // 'positive', 'negative', 'neutral'
    }

    public class EmployeeActivityDataDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeEmail { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public float AllocatedPercentage { get; set; }
        public int MonthsActive { get; set; }
        public List<AllocationHistoryDto> AllocationHistory { get; set; } = new List<AllocationHistoryDto>();
    }

    public class AllocationHistoryDto
    {
        public string Month { get; set; } = string.Empty;
        public float Percentage { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
    }
}