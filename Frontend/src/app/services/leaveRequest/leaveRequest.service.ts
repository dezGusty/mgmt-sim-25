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


export interface RemainingLeaveDaysResponse {
  remainingDays: number;
  totalDays: number;
  usedDays: number;
  leaveRequestTypeId: number;
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

  cancelLeaveRequestByEmployee(requestId: number): Observable<{Message: string}> {
        return this.http.patch<{Message: string}>(`${this.apiUrl}/by-employee/${requestId}`, {}, {
            withCredentials: true
        });
  }

  getCurrentUserRemainingLeaveDays(leaveRequestTypeId: number, year?: number): Observable<IApiResponse<RemainingLeaveDaysResponse>> {
  const yearParam = year ? `?year=${year}` : '';
  const url = `${this.apiUrl}/by-employee/remaining-days/${leaveRequestTypeId}${yearParam}`;
  
 
  
  return this.http.get<IApiResponse<RemainingLeaveDaysResponse>>(url, {
    withCredentials: true,
  });
}

  getCurrentUserRemainingLeaveDaysForPeriod(
    leaveRequestTypeId: number, 
    startDate: string, 
    endDate: string
  ): Observable<IApiResponse<RemainingLeaveDaysResponse>> {
    const url = `${this.apiUrl}/by-employee/remaining-days-for-period/${leaveRequestTypeId}?startDate=${startDate}&endDate=${endDate}`;
  
   
  
    return this.http.get<IApiResponse<RemainingLeaveDaysResponse>>(url, {
      withCredentials: true,
    });
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
