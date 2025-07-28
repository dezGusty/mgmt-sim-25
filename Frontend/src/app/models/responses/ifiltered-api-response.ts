  export interface IFilteredApiResponse<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}