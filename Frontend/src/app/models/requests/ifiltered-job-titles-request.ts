import { IQueryParams } from "./iquery-params";

export interface IFilteredJobTitlesRequest {
    jobTitleName? : string;
    departmentName?: string;
    params: IQueryParams;
}