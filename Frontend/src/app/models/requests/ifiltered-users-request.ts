import { IQueryParams } from "./iquery-params";
import { UserActivityStatus } from "../enums/user-activity-status";

export interface IFilteredUsersRequest {
    lastName?: string;
    email?: string;
    department?: string;
    jobTitle?: string;
    globalSearch?: string;
    employeeName?: string;
    employeeEmail?: string;
    managerName?: string;
    managerEmail?: string;
    unassignedName?: string;
    status?: UserActivityStatus;
    params: IQueryParams;
}