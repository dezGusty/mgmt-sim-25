export interface IFilteredApiResponse<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}