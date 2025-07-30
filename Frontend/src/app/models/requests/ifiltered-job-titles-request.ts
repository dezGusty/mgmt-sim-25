import { IQueryParams } from "./iquery-params";

export interface IFilteredJobTitlesRequest {
    jobTitleName? : string;
    departmentName?: string;
    isPaid?: boolean;
    params: IQueryParams;
}