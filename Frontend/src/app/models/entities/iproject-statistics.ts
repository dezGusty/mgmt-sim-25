export interface IMonthlyAllocationData {
  month: string; // YYYY-MM format
  allocations: number;
  deallocations: number;
  totalEmployees: number;
  totalFTEs: number;
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