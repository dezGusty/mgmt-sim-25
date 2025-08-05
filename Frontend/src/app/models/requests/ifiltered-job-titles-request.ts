import { IQueryParams } from "./iquery-params";
import { JobTitleActivityStatus } from "../enums/job-title-activity-status";

export interface IFilteredJobTitlesRequest {
    jobTitleName? : string;
    activityStatus : JobTitleActivityStatus;
    params: IQueryParams;
}