using System;
using System.ComponentModel.DataAnnotations;

namespace ManagementSimulator.Core.Dtos.Requests.ProjectStatistics
{
    public class ProjectStatisticsRequestDto
    {
        [Required]
        public int ProjectId { get; set; }

        public int? FiscalYear { get; set; } // Optional, defaults to current fiscal year

        public string? Month { get; set; } // Optional YYYY-MM format for specific month data
    }

    public class ProjectStatisticsOverviewRequestDto
    {
        public int? FiscalYear { get; set; } // Optional, defaults to current fiscal year

        public string? Month { get; set; } // Optional YYYY-MM format for specific month data

        public int[]? ProjectIds { get; set; } // Optional, for filtering specific projects
    }

    public class ProjectAllocationChartRequestDto
    {
        [Required]
        public int ProjectId { get; set; }

        [Required]
        public string Month { get; set; } = string.Empty; // YYYY-MM format

        public int? FiscalYear { get; set; } // Optional, defaults to current fiscal year

        public bool IncludeTrends { get; set; } = true;
    }

    public class ProjectBudgetChartRequestDto
    {
        [Required]
        public int ProjectId { get; set; }

        [Required]
        public string Month { get; set; } = string.Empty; // YYYY-MM format

        public int? FiscalYear { get; set; } // Optional, defaults to current fiscal year

        public bool IncludeTrends { get; set; } = true;
    }
}