export interface IFilteredApiResponse<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalPages: number;
  totalCount?: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}