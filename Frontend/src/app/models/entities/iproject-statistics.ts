export interface IMonthlyAllocationData {
  month: string; // YYYY-MM format
  allocations: number;
  deallocations: number;
  totalEmployees: number;
  totalFTEs: number;
  dailyData: IDailyAllocationData[];
  trendComparison?: IMonthTrendComparison;
}

export interface IDailyAllocationData {
  date: string; // YYYY-MM-DD format
  totalEmployees: number;
  totalFTEs: number;
  cumulativeAllocations: number;
  cumulativeDeallocations: number;
}

export interface IEmployeeActivityData {
  employeeId: number;
  employeeName: string;
  employeeEmail: string;
  jobTitle: string;
  allocatedPercentage: number;
  monthsActive: number;
  allocationHistory: {
    month: string;
    percentage: number;
    startDate?: string;
    endDate?: string;
  }[];
}

export interface IProjectMilestone {
  date: string;
  type: 'project_start' | 'project_end' | 'budget_change' | 'major_assignment';
  description: string;
  impact: 'positive' | 'negative' | 'neutral';
}

export interface IBudgetUtilization {
  month: string;
  budgetedFTEs: number;
  actualFTEs: number;
  utilizationPercentage: number;
  variance: number;
  dailyData: IDailyBudgetData[];
  trendComparison?: IMonthTrendComparison;
}

export interface IDailyBudgetData {
  date: string; // YYYY-MM-DD format
  budgetedFTEs: number;
  actualFTEs: number;
  utilizationPercentage: number;
  variance: number;
}

export interface IMonthTrendComparison {
  previousMonth: string;
  currentMonth: string;
  allocationsChange: number; // percentage change
  budgetUtilizationChange: number; // percentage change
  employeeCountChange: number; // absolute change
  ftesChange: number; // absolute change
  trend: 'improving' | 'declining' | 'stable';
}

export interface IFiscalYear {
  startDate: Date; // October 1st
  endDate: Date; // September 30th
  year: number; // The year when fiscal year starts (e.g., 2024 for FY 2024-2025)
  label: string; // "FY 2024-2025"
  daysRemaining: number; // Days until next fiscal year starts
  isCurrentFiscalYear: boolean; // Whether this is the current active fiscal year
}

export interface IProjectActivitySummary {
  totalEmployeesEver: number;
  currentEmployees: number;
  averageAllocationPercentage: number;
  peakEmployeeCount: number;
  peakEmployeeMonth: string;
  totalAllocationEvents: number;
  totalDeallocationEvents: number;
  projectDuration: number; // in months
}

export interface IProjectStatistics {
  projectId: number;
  projectName: string;
  fiscalYear: IFiscalYear;
  monthlyAllocationData: IMonthlyAllocationData[];
  employeeActivity: IEmployeeActivityData[];
  budgetUtilization: IBudgetUtilization[];
  milestones: IProjectMilestone[];
  summary: IProjectActivitySummary;
  lastUpdated: Date;
}

export interface IAllocationEvent {
  id: number;
  employeeId: number;
  employeeName: string;
  employeeEmail: string;
  action: 'assigned' | 'removed' | 'percentage_changed';
  oldPercentage?: number;
  newPercentage?: number;
  timestamp: Date;
  auditLogId?: number;
}

export interface IProjectStatisticsOverview {
  fiscalYear: IFiscalYear;
  selectedMonth?: string; // YYYY-MM format
  availableMonths: string[]; // All months in fiscal year with data
  totalProjects: number;
  totalBudgetedFTEs: number;
  totalAllocatedFTEs: number;
  averageUtilization: number;
}

export interface IChartDataPoint {
  date: string;
  value: number;
  label?: string;
}

export interface IStatisticsChartData {
  title: string;
  data: IChartDataPoint[];
  type: 'line' | 'bar' | 'area';
  color: string;
  previousPeriodData?: IChartDataPoint[];
}