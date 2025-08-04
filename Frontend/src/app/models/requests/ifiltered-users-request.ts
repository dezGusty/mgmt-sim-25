import { IQueryParams } from "./iquery-params";

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
    params: IQueryParams;
}