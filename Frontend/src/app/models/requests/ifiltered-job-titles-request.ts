import { IQueryParams } from "./iquery-params";

export interface IFilteredJobTitlesRequest {
    jobTitleName? : string;
    isPaid?: boolean;
    params: IQueryParams;
}