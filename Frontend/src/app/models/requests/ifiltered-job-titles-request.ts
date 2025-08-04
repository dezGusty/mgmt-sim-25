import { IQueryParams } from "./iquery-params";
import { JobTitleActivityStatus } from "../enums/JobTitleActivityStatus";

export interface IFilteredJobTitlesRequest {
    jobTitleName? : string;
    activityStatus : JobTitleActivityStatus;
    params: IQueryParams;
}