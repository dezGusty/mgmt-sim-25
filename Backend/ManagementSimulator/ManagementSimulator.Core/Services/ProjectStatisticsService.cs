using ManagementSimulator.Core.Dtos.Requests.ProjectStatistics;
using ManagementSimulator.Core.Dtos.Responses.ProjectStatistics;
using ManagementSimulator.Core.Services.Interfaces;
using ManagementSimulator.Database.Repositories.Intefaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ManagementSimulator.Database.Entities;
using Newtonsoft.Json;

namespace ManagementSimulator.Core.Services
{
    public class ProjectStatisticsService : IProjectStatisticsService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IAuditRepository _auditRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ProjectStatisticsService> _logger;

        public ProjectStatisticsService(
            IProjectRepository projectRepository,
            IAuditRepository auditRepository,
            IUserRepository userRepository,
            ILogger<ProjectStatisticsService> logger)
        {
            _projectRepository = projectRepository;
            _auditRepository = auditRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<FiscalYearDto> GetCurrentFiscalYearAsync()
        {
            var currentDate = DateTime.UtcNow;
            var fiscalYear = currentDate.Month >= 10 ? currentDate.Year : currentDate.Year - 1;
            return await GetFiscalYearAsync(fiscalYear);
        }

        public async Task<FiscalYearDto> GetFiscalYearAsync(int year)
        {
            return await Task.FromResult(new FiscalYearDto
            {
                Year = year,
                StartDate = new DateTime(year, 10, 1),
                EndDate = new DateTime(year + 1, 9, 30),
                Label = $"FY {year}-{year + 1}"
            });
        }

        public async Task<List<string>> GetAvailableMonthsForProjectAsync(int projectId, int fiscalYear)
        {
            var fiscalYearInfo = await GetFiscalYearAsync(fiscalYear);

            // Get audit logs for the project within fiscal year
            var auditLogs = await _auditRepository.GetAuditLogsByEntityAsync("Project", projectId,
                fiscalYearInfo.StartDate, fiscalYearInfo.EndDate);

            var months = new HashSet<string>();

            foreach (var log in auditLogs)
            {
                var monthKey = log.Timestamp.ToString("yyyy-MM");
                months.Add(monthKey);
            }

            return months.OrderBy(m => m).ToList();
        }

        public async Task<ProjectStatisticsResponseDto> GetProjectStatisticsAsync(ProjectStatisticsRequestDto request)
        {
            var project = await _projectRepository.GetProjectByIdAsync(request.ProjectId);
            if (project == null)
            {
                throw new ArgumentException($"Project with ID {request.ProjectId} not found.");
            }

            var fiscalYear = request.FiscalYear.HasValue
                ? await GetFiscalYearAsync(request.FiscalYear.Value)
                : await GetCurrentFiscalYearAsync();

            var response = new ProjectStatisticsResponseDto
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                FiscalYear = fiscalYear,
                LastUpdated = DateTime.UtcNow
            };

            // Get audit logs for the project within fiscal year
            var auditLogs = await _auditRepository.GetAuditLogsByEntityAsync("Project", project.Id,
                fiscalYear.StartDate, fiscalYear.EndDate);

            // Process monthly allocation data
            response.MonthlyAllocationData = await GetMonthlyAllocationDataAsync(project.Id, fiscalYear, auditLogs);

            // Process budget utilization
            response.BudgetUtilization = await GetBudgetUtilizationDataAsync(project.Id, fiscalYear, auditLogs);

            // Process employee activity
            response.EmployeeActivity = await GetEmployeeActivityDataAsync(project.Id, fiscalYear, auditLogs);

            // Process milestones
            response.Milestones = await GetProjectMilestonesAsync(project.Id, fiscalYear, auditLogs);

            // Calculate summary
            response.Summary = await CalculateProjectSummaryAsync(project.Id, fiscalYear, response);

            return response;
        }

        public async Task<ProjectStatisticsOverviewDto> GetProjectStatisticsOverviewAsync(ProjectStatisticsOverviewRequestDto request)
        {
            var fiscalYear = request.FiscalYear.HasValue
                ? await GetFiscalYearAsync(request.FiscalYear.Value)
                : await GetCurrentFiscalYearAsync();

            var projects = request.ProjectIds?.Any() == true
                ? await _projectRepository.GetProjectsByIdsAsync(request.ProjectIds)
                : await _projectRepository.GetAllAsync();

            var overview = new ProjectStatisticsOverviewDto
            {
                FiscalYear = fiscalYear,
                SelectedMonth = request.Month,
                TotalProjects = projects.Count
            };

            float totalBudgetedFTEs = 0;
            float totalAllocatedFTEs = 0;
            var allAvailableMonths = new HashSet<string>();

            foreach (var project in projects)
            {
                totalBudgetedFTEs += project.BudgetedFTEs;

                // Get current allocations for the project
                var currentAllocations = await _projectRepository.GetProjectUsersAsync(project.Id);
                var projectAllocatedFTEs = currentAllocations.Sum(up => up.TimePercentagePerProject / 100f);
                totalAllocatedFTEs += projectAllocatedFTEs;

                // Get available months for this project
                var projectMonths = await GetAvailableMonthsForProjectAsync(project.Id, fiscalYear.Year);
                foreach (var month in projectMonths)
                {
                    allAvailableMonths.Add(month);
                }
            }

            overview.TotalBudgetedFTEs = totalBudgetedFTEs;
            overview.TotalAllocatedFTEs = totalAllocatedFTEs;
            overview.AverageUtilization = totalBudgetedFTEs > 0 ? (totalAllocatedFTEs / totalBudgetedFTEs) * 100 : 0;
            overview.AvailableMonths = allAvailableMonths.OrderBy(m => m).ToList();

            return overview;
        }

        public async Task<StatisticsChartDataDto> GetProjectAllocationChartDataAsync(ProjectAllocationChartRequestDto request)
        {
            var fiscalYear = request.FiscalYear.HasValue
                ? await GetFiscalYearAsync(request.FiscalYear.Value)
                : await GetCurrentFiscalYearAsync();

            var startDate = DateTime.ParseExact(request.Month + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var auditLogs = await _auditRepository.GetAuditLogsByEntityAsync("UserProject", null, startDate, endDate);

            // Filter for specific project
            var projectLogs = auditLogs.Where(log =>
                log.AdditionalData?.Contains($"\"ProjectId\":{request.ProjectId}") == true ||
                log.EntityName?.Contains($"Project:{request.ProjectId}") == true
            ).ToList();

            var chartData = new StatisticsChartDataDto
            {
                Title = $"Daily Allocations for {request.Month}",
                Type = "line",
                Color = "#4F46E5",
                Data = new List<ChartDataPointDto>()
            };

            // Generate daily data points
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dayLogs = projectLogs.Where(log => log.Timestamp.Date == date.Date);
                var allocations = dayLogs.Count(log => log.Action?.Contains("CREATE") == true || log.Action?.Contains("assigned") == true);

                chartData.Data.Add(new ChartDataPointDto
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Value = allocations,
                    Label = date.ToString("dd/MM")
                });
            }

            if (request.IncludeTrends)
            {
                // Get previous month data for comparison
                var prevMonth = startDate.AddMonths(-1);
                var prevMonthRequest = new ProjectAllocationChartRequestDto
                {
                    ProjectId = request.ProjectId,
                    Month = prevMonth.ToString("yyyy-MM"),
                    FiscalYear = request.FiscalYear,
                    IncludeTrends = false
                };
                var prevMonthData = await GetProjectAllocationChartDataAsync(prevMonthRequest);
                chartData.PreviousPeriodData = prevMonthData.Data;
            }

            return chartData;
        }

        public async Task<StatisticsChartDataDto> GetProjectBudgetChartDataAsync(ProjectBudgetChartRequestDto request)
        {
            var project = await _projectRepository.GetProjectByIdAsync(request.ProjectId);
            if (project == null)
            {
                throw new ArgumentException($"Project with ID {request.ProjectId} not found.");
            }

            var startDate = DateTime.ParseExact(request.Month + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var chartData = new StatisticsChartDataDto
            {
                Title = $"Daily Budget Utilization for {request.Month}",
                Type = "area",
                Color = "#059669",
                Data = new List<ChartDataPointDto>()
            };

            // Generate daily data points
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // Get project allocations at this point in time
                var currentAllocations = await _projectRepository.GetProjectUsersAsync(project.Id);
                var actualFTEs = currentAllocations.Sum(up => up.TimePercentagePerProject / 100f);
                var utilization = project.BudgetedFTEs > 0 ? (actualFTEs / project.BudgetedFTEs) * 100 : 0;

                chartData.Data.Add(new ChartDataPointDto
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Value = utilization,
                    Label = date.ToString("dd/MM")
                });
            }

            if (request.IncludeTrends)
            {
                // Get previous month data for comparison
                var prevMonth = startDate.AddMonths(-1);
                var prevMonthRequest = new ProjectBudgetChartRequestDto
                {
                    ProjectId = request.ProjectId,
                    Month = prevMonth.ToString("yyyy-MM"),
                    FiscalYear = request.FiscalYear,
                    IncludeTrends = false
                };
                var prevMonthData = await GetProjectBudgetChartDataAsync(prevMonthRequest);
                chartData.PreviousPeriodData = prevMonthData.Data;
            }

            return chartData;
        }

        public async Task<List<AllocationEventDto>> GetProjectAllocationEventsAsync(int projectId, DateTime startDate, DateTime endDate)
        {
            var auditLogs = await _auditRepository.GetAuditLogsByEntityAsync("UserProject", null, startDate, endDate);

            var events = new List<AllocationEventDto>();

            foreach (var log in auditLogs)
            {
                try
                {
                    if (log.AdditionalData?.Contains($"\"ProjectId\":{projectId}") == true)
                    {
                        var user = await _userRepository.GetUserByIdAsync(log.UserId);
                        var additionalData = JsonConvert.DeserializeObject<dynamic>(log.AdditionalData ?? "{}");

                        events.Add(new AllocationEventDto
                        {
                            Id = log.Id,
                            EmployeeId = log.UserId,
                            EmployeeName = user?.FirstName + " " + user?.LastName ?? "Unknown",
                            EmployeeEmail = user?.Email ?? log.UserEmail,
                            Action = DetermineActionType(log.Action),
                            OldPercentage = ExtractPercentage(log.OldValues),
                            NewPercentage = ExtractPercentage(log.NewValues),
                            Timestamp = log.Timestamp,
                            AuditLogId = log.Id
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Error processing audit log {log.Id} for allocation events");
                }
            }

            return events.OrderByDescending(e => e.Timestamp).ToList();
        }

        #region Private Helper Methods

        private async Task<List<MonthlyAllocationDataDto>> GetMonthlyAllocationDataAsync(int projectId, FiscalYearDto fiscalYear, List<AuditLog> auditLogs)
        {
            var monthlyData = new List<MonthlyAllocationDataDto>();

            for (var date = fiscalYear.StartDate; date <= fiscalYear.EndDate; date = date.AddMonths(1))
            {
                var monthKey = date.ToString("yyyy-MM");
                var monthEnd = date.AddMonths(1).AddDays(-1);

                var monthLogs = auditLogs.Where(log =>
                    log.Timestamp >= date && log.Timestamp <= monthEnd &&
                    log.AdditionalData?.Contains($"\"ProjectId\":{projectId}") == true
                ).ToList();

                var allocations = monthLogs.Count(log => log.Action?.Contains("CREATE") == true || log.Action?.Contains("assigned") == true);
                var deallocations = monthLogs.Count(log => log.Action?.Contains("DELETE") == true || log.Action?.Contains("removed") == true);

                // Get current employee count and FTEs at end of month
                var currentUsers = await _projectRepository.GetProjectUsersAsync(projectId);
                var totalFTEs = currentUsers.Sum(up => up.TimePercentagePerProject / 100f);

                var monthData = new MonthlyAllocationDataDto
                {
                    Month = monthKey,
                    Allocations = allocations,
                    Deallocations = deallocations,
                    TotalEmployees = currentUsers.Count,
                    TotalFTEs = totalFTEs,
                    DailyData = await GetDailyAllocationDataAsync(projectId, date, monthEnd)
                };

                monthlyData.Add(monthData);
            }

            // Add trend comparisons
            for (int i = 1; i < monthlyData.Count; i++)
            {
                monthlyData[i].TrendComparison = CalculateTrendComparison(monthlyData[i - 1], monthlyData[i]);
            }

            return monthlyData;
        }

        private async Task<List<DailyAllocationDataDto>> GetDailyAllocationDataAsync(int projectId, DateTime startDate, DateTime endDate)
        {
            var dailyData = new List<DailyAllocationDataDto>();
            var cumulativeAllocations = 0;
            var cumulativeDeallocations = 0;

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // This is a simplified version - in practice, you'd track daily changes
                var currentUsers = await _projectRepository.GetProjectUsersAsync(projectId);
                var totalFTEs = currentUsers.Sum(up => up.TimePercentagePerProject / 100f);

                dailyData.Add(new DailyAllocationDataDto
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    TotalEmployees = currentUsers.Count,
                    TotalFTEs = totalFTEs,
                    CumulativeAllocations = cumulativeAllocations,
                    CumulativeDeallocations = cumulativeDeallocations
                });
            }

            return dailyData;
        }

        private async Task<List<BudgetUtilizationDto>> GetBudgetUtilizationDataAsync(int projectId, FiscalYearDto fiscalYear, List<AuditLog> auditLogs)
        {
            var project = await _projectRepository.GetProjectByIdAsync(projectId);
            var budgetData = new List<BudgetUtilizationDto>();

            for (var date = fiscalYear.StartDate; date <= fiscalYear.EndDate; date = date.AddMonths(1))
            {
                var monthKey = date.ToString("yyyy-MM");
                var monthEnd = date.AddMonths(1).AddDays(-1);

                var currentUsers = await _projectRepository.GetProjectUsersAsync(projectId);
                var actualFTEs = currentUsers.Sum(up => up.TimePercentagePerProject / 100f);
                var budgetedFTEs = project?.BudgetedFTEs ?? 0;
                var utilization = budgetedFTEs > 0 ? (actualFTEs / budgetedFTEs) * 100 : 0;
                var variance = actualFTEs - budgetedFTEs;

                var budgetMonth = new BudgetUtilizationDto
                {
                    Month = monthKey,
                    BudgetedFTEs = budgetedFTEs,
                    ActualFTEs = actualFTEs,
                    UtilizationPercentage = utilization,
                    Variance = variance,
                    DailyData = await GetDailyBudgetDataAsync(projectId, date, monthEnd, budgetedFTEs)
                };

                budgetData.Add(budgetMonth);
            }

            // Add trend comparisons
            for (int i = 1; i < budgetData.Count; i++)
            {
                budgetData[i].TrendComparison = CalculateBudgetTrendComparison(budgetData[i - 1], budgetData[i]);
            }

            return budgetData;
        }

        private async Task<List<DailyBudgetDataDto>> GetDailyBudgetDataAsync(int projectId, DateTime startDate, DateTime endDate, float budgetedFTEs)
        {
            var dailyData = new List<DailyBudgetDataDto>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var currentUsers = await _projectRepository.GetProjectUsersAsync(projectId);
                var actualFTEs = currentUsers.Sum(up => up.TimePercentagePerProject / 100f);
                var utilization = budgetedFTEs > 0 ? (actualFTEs / budgetedFTEs) * 100 : 0;
                var variance = actualFTEs - budgetedFTEs;

                dailyData.Add(new DailyBudgetDataDto
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    BudgetedFTEs = budgetedFTEs,
                    ActualFTEs = actualFTEs,
                    UtilizationPercentage = utilization,
                    Variance = variance
                });
            }

            return dailyData;
        }

        private async Task<List<EmployeeActivityDataDto>> GetEmployeeActivityDataAsync(int projectId, FiscalYearDto fiscalYear, List<AuditLog> auditLogs)
        {
            var currentUsers = await _projectRepository.GetProjectUsersAsync(projectId);
            var employeeActivities = new List<EmployeeActivityDataDto>();

            foreach (var userProject in currentUsers)
            {
                var user = userProject.User;
                if (user == null) continue;

                var userLogs = auditLogs.Where(log => log.UserId == user.Id).ToList();

                var activity = new EmployeeActivityDataDto
                {
                    EmployeeId = user.Id,
                    EmployeeName = $"{user.FirstName} {user.LastName}",
                    EmployeeEmail = user.Email,
                    JobTitle = user.Title?.Name ?? "Unknown",
                    AllocatedPercentage = userProject.TimePercentagePerProject,
                    MonthsActive = CalculateMonthsActive(userLogs, fiscalYear),
                    AllocationHistory = BuildAllocationHistory(userLogs, fiscalYear)
                };

                employeeActivities.Add(activity);
            }

            return employeeActivities;
        }

        private async Task<List<ProjectMilestoneDto>> GetProjectMilestonesAsync(int projectId, FiscalYearDto fiscalYear, List<AuditLog> auditLogs)
        {
            var project = await _projectRepository.GetProjectByIdAsync(projectId);
            var milestones = new List<ProjectMilestoneDto>();

            // Add project start/end milestones if within fiscal year
            if (project != null && project.StartDate >= fiscalYear.StartDate && project.StartDate <= fiscalYear.EndDate)
            {
                milestones.Add(new ProjectMilestoneDto
                {
                    Date = project.StartDate.ToString("yyyy-MM-dd"),
                    Type = "project_start",
                    Description = $"Project {project.Name} started",
                    Impact = "positive"
                });
            }

            if (project != null && project.EndDate >= fiscalYear.StartDate && project.EndDate <= fiscalYear.EndDate)
            {
                milestones.Add(new ProjectMilestoneDto
                {
                    Date = project.EndDate.ToString("yyyy-MM-dd"),
                    Type = "project_end",
                    Description = $"Project {project.Name} ended",
                    Impact = "neutral"
                });
            }

            // Add major allocation events as milestones
            var majorEvents = auditLogs.Where(log =>
                log.Action?.Contains("CREATE") == true ||
                log.Action?.Contains("DELETE") == true
            ).Take(10); // Limit to most significant events

            foreach (var eventLog in majorEvents)
            {
                milestones.Add(new ProjectMilestoneDto
                {
                    Date = eventLog.Timestamp.ToString("yyyy-MM-dd"),
                    Type = "major_assignment",
                    Description = eventLog.Description ?? $"Employee allocation change",
                    Impact = eventLog.Action?.Contains("CREATE") == true ? "positive" : "negative"
                });
            }

            return milestones.OrderBy(m => m.Date).ToList();
        }

        private async Task<ProjectActivitySummaryDto> CalculateProjectSummaryAsync(int projectId, FiscalYearDto fiscalYear, ProjectStatisticsResponseDto response)
        {
            var currentUsers = await _projectRepository.GetProjectUsersAsync(projectId);

            var summary = new ProjectActivitySummaryDto
            {
                CurrentEmployees = currentUsers.Count,
                TotalEmployeesEver = response.EmployeeActivity.Count,
                AverageAllocationPercentage = currentUsers.Any() ? currentUsers.Average(up => up.TimePercentagePerProject) : 0,
                TotalAllocationEvents = response.MonthlyAllocationData.Sum(m => m.Allocations),
                TotalDeallocationEvents = response.MonthlyAllocationData.Sum(m => m.Deallocations),
                ProjectDuration = response.MonthlyAllocationData.Count
            };

            if (response.MonthlyAllocationData.Any())
            {
                var peakMonth = response.MonthlyAllocationData.OrderByDescending(m => m.TotalEmployees).First();
                summary.PeakEmployeeCount = peakMonth.TotalEmployees;
                summary.PeakEmployeeMonth = peakMonth.Month;
            }

            return summary;
        }

        private MonthTrendComparisonDto CalculateTrendComparison(MonthlyAllocationDataDto previous, MonthlyAllocationDataDto current)
        {
            var allocationsChange = previous.Allocations > 0
                ? ((float)(current.Allocations - previous.Allocations) / previous.Allocations) * 100
                : 0;

            var employeeChange = current.TotalEmployees - previous.TotalEmployees;
            var ftesChange = current.TotalFTEs - previous.TotalFTEs;

            var trend = "stable";
            if (allocationsChange > 5) trend = "improving";
            else if (allocationsChange < -5) trend = "declining";

            return new MonthTrendComparisonDto
            {
                PreviousMonth = previous.Month,
                CurrentMonth = current.Month,
                AllocationsChange = allocationsChange,
                BudgetUtilizationChange = 0, // Will be calculated in budget method
                EmployeeCountChange = employeeChange,
                FtesChange = ftesChange,
                Trend = trend
            };
        }

        private MonthTrendComparisonDto CalculateBudgetTrendComparison(BudgetUtilizationDto previous, BudgetUtilizationDto current)
        {
            var utilizationChange = current.UtilizationPercentage - previous.UtilizationPercentage;
            var ftesChange = current.ActualFTEs - previous.ActualFTEs;

            var trend = "stable";
            if (utilizationChange > 5) trend = "improving";
            else if (utilizationChange < -5) trend = "declining";

            return new MonthTrendComparisonDto
            {
                PreviousMonth = previous.Month,
                CurrentMonth = current.Month,
                AllocationsChange = 0,
                BudgetUtilizationChange = utilizationChange,
                EmployeeCountChange = 0,
                FtesChange = ftesChange,
                Trend = trend
            };
        }

        private int CalculateMonthsActive(List<AuditLog> userLogs, FiscalYearDto fiscalYear)
        {
            var months = userLogs
                .Where(log => log.Timestamp >= fiscalYear.StartDate && log.Timestamp <= fiscalYear.EndDate)
                .Select(log => log.Timestamp.ToString("yyyy-MM"))
                .Distinct()
                .Count();

            return months;
        }

        private List<AllocationHistoryDto> BuildAllocationHistory(List<AuditLog> userLogs, FiscalYearDto fiscalYear)
        {
            var history = new List<AllocationHistoryDto>();

            // Group logs by month and extract percentage changes
            var monthlyLogs = userLogs
                .Where(log => log.Timestamp >= fiscalYear.StartDate && log.Timestamp <= fiscalYear.EndDate)
                .GroupBy(log => log.Timestamp.ToString("yyyy-MM"))
                .OrderBy(g => g.Key);

            foreach (var monthGroup in monthlyLogs)
            {
                var percentage = ExtractPercentage(monthGroup.Last().NewValues) ?? 0;

                history.Add(new AllocationHistoryDto
                {
                    Month = monthGroup.Key,
                    Percentage = percentage,
                    StartDate = monthGroup.Min(log => log.Timestamp).ToString("yyyy-MM-dd"),
                    EndDate = monthGroup.Max(log => log.Timestamp).ToString("yyyy-MM-dd")
                });
            }

            return history;
        }

        private string DetermineActionType(string action)
        {
            if (action?.Contains("CREATE") == true) return "assigned";
            if (action?.Contains("DELETE") == true) return "removed";
            if (action?.Contains("UPDATE") == true) return "percentage_changed";
            return "unknown";
        }

        private float? ExtractPercentage(string? jsonValues)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonValues)) return null;

                var data = JsonConvert.DeserializeObject<dynamic>(jsonValues);
                if (data?.TimePercentagePerProject != null)
                {
                    return (float)data.TimePercentagePerProject;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extracting percentage from JSON values: {JsonValues}", jsonValues);
            }

            return null;
        }

        #endregion
    }
}