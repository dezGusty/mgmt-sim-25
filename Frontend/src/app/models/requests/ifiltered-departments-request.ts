import { IQueryParams } from "./iquery-params";
import { SearchType } from "../enums/search-type";

export interface IFilteredDepartmentsRequest {
    name?: string;
    activityStatus?: SearchType;
    params : IQueryParams; 
}