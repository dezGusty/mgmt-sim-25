import { IQueryParams } from "./iquery-params";

export interface IFilteredDepartmentsRequest {
    name?: string;
    includeDeleted?: boolean;
    params : IQueryParams; 
}