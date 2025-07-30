import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { LeaveRequest } from "../../models/entities/iLeave-request";
import { environment } from "../../../environments/environment";
import { IApiResponse } from "../../models/responses/iapi-response";
import { RequestStatus } from "../../models/enums/RequestStatus";


export interface CreateLeaveRequestByEmployeeDto {
    leaveRequestTypeId: number;
    startDate: string; 
    endDate: string;   
    reason?: string;
    requestStatus?: RequestStatus;
}

@Injectable({
    providedIn: "root"
})
export class LeaveRequestService {
    private apiUrl = `${environment.apiUrl}/LeaveRequests`;

    constructor(private http: HttpClient) {}

    getCurrentUserLeaveRequests(): Observable<IApiResponse<LeaveRequest[]>> {
        return this.http.get<IApiResponse<LeaveRequest[]>>(`${this.apiUrl}/by-employee`, {
            withCredentials: true
        });
    }

    addLeaveRequestByEmployee(request: CreateLeaveRequestByEmployeeDto): Observable<IApiResponse<LeaveRequest>> {
        return this.http.post<IApiResponse<LeaveRequest>>(`${this.apiUrl}/by-employee`, request, {
            withCredentials: true
        });
    }

    cancelLeaveRequestByEmployee(requestId: number): Observable<{Message: string}> {
        return this.http.patch<{Message: string}>(`${this.apiUrl}/by-employee/${requestId}`, {}, {
            withCredentials: true
        });
    }

  

    getLeaveBalance(userId: number): Observable<any> {
        return this.http.get<any>(`${this.apiUrl}/balance/${userId}`, {
            withCredentials: true
        });
    }

    getCurrentUserLeaveBalance(): Observable<any> {
        return this.http.get<any>(`${this.apiUrl}/balance/by-employee`, {
            withCredentials: true
        });
    }
}