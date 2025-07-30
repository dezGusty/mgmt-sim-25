import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { LeaveRequest } from '../../models/entities/iLeave-request';
import { environment } from '../../../environments/environment';
import { IApiResponse } from '../../models/responses/iapi-response';
import { RequestStatus } from '../../models/enums/RequestStatus';

export interface CreateLeaveRequestByEmployeeDto {
  leaveRequestTypeId: number;
  startDate: string;
  endDate: string;
  reason?: string;
  requestStatus?: RequestStatus;
}

@Injectable({
  providedIn: 'root',
})
export class LeaveRequestService {
  private apiUrl = `${environment.apiUrl}/LeaveRequests`;

  constructor(private http: HttpClient) {}

  getCurrentUserLeaveRequests(): Observable<IApiResponse<LeaveRequest[]>> {
    return this.http.get<IApiResponse<LeaveRequest[]>>(
      `${this.apiUrl}/by-employee`,
      {
        withCredentials: true,
      }
    );
  }

  addLeaveRequestByEmployee(
    request: CreateLeaveRequestByEmployeeDto
  ): Observable<IApiResponse<LeaveRequest>> {
    return this.http.post<IApiResponse<LeaveRequest>>(
      `${this.apiUrl}/by-employee`,
      request,
      {
        withCredentials: true,
      }
    );
  }

  getUserLeaveRequests(
    userId: number
  ): Observable<IApiResponse<LeaveRequest[]>> {
    return this.http.get<IApiResponse<LeaveRequest[]>>(
      `${this.apiUrl}/user/${userId}`,
      {
        withCredentials: true,
      }
    );
  }

  addLeaveRequest(
    request: LeaveRequest
  ): Observable<IApiResponse<LeaveRequest>> {
    return this.http.post<IApiResponse<LeaveRequest>>(this.apiUrl, request, {
      withCredentials: true,
    });
  }

  cancelLeaveRequest(requestId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${requestId}`, {
      withCredentials: true,
    });
  }

  getLeaveBalance(userId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/balance/${userId}`, {
      withCredentials: true,
    });
  }

  getCurrentUserLeaveBalance(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/balance/by-employee`, {
      withCredentials: true,
    });
  }

  getRemainingLeaveDaysForPeriod(
    userId: number,
    leaveRequestTypeId: number,
    startDate: string,
    endDate: string
  ): Observable<any> {
    return this.http.get<any>(
      `${this.apiUrl}/remaining-days-for-period/${userId}/${leaveRequestTypeId}?startDate=${startDate}&endDate=${endDate}`,
      {
        withCredentials: true,
      }
    );
  }
}
