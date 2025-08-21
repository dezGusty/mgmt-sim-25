export interface IFilteredProjectsRequest {
  searchTerm?: string;
  isActive?: boolean;
  startDateFrom?: string;
  startDateTo?: string;
  endDateFrom?: string;
  endDateTo?: string;
  minBudgetedFTEs?: number;
  maxBudgetedFTEs?: number;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
}