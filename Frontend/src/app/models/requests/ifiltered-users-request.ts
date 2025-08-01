import { IQueryParams } from "./iquery-params";

export interface IFilteredUsersRequest {
    lastName?: string;
    email?: string;
    department?: string;
    jobTitle?: string;
    globalSearch?: string;
    params: IQueryParams;
}