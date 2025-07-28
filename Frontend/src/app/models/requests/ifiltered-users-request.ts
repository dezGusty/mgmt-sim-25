import { IQueryParams } from "./iquery-params";

export interface IFilteredUsersRequest {
    lastName?: string;
    email?: string;
    params: IQueryParams;
}