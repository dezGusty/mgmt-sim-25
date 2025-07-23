import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { LeaveRequest } from "../../models/entities/LeaveRequest";
import { environment } from "../../../environments/environment";


@Injectable({
    providedIn: "root"
})
export class LeaveRequestService {
    private apiUrl = `${environment.apiUrl}/LeaveRequests`;

    constructor(private http: HttpClient) {}

    getUserLeaveRequests(userId: number): Observable<LeaveRequest[]> {
        return this.http.get<LeaveRequest[]>(`${this.apiUrl}/user/${userId}`);
    }

    addLeaveRequest(request: LeaveRequest): Observable<LeaveRequest> {
        return this.http.post<LeaveRequest>(this.apiUrl, request);
    }

    cancelLeaveRequest(requestId: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${requestId}`);
    }

    getLeaveBalance(userId: number): Observable<any> {
        return this.http.get<any>(`${this.apiUrl}/balance/${userId}`);
    }

}