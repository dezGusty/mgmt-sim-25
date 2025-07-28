import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of, tap } from 'rxjs';
import { IApiResponse } from '../../models/responses/iapi-response';
import { IUser } from '../../models/entities/iuser';
import { ILeaveRequest } from '../../models/leave-request';

@Injectable({
  providedIn: 'root',
})
export class LeaveRequests {
  private apiUrl = 'https://localhost:7275/api/LeaveRequests';

  constructor(private http: HttpClient) {}

  fetchByManager(): Observable<IApiResponse<IUser[]>> {
    return this.http.get<IApiResponse<IUser[]>>(`${this.apiUrl}/by-manager`, {
      withCredentials: true,
    });
  }

  addLeaveRequest(data: {
    userId: number;
    leaveRequestTypeId: number;
    startDate: string;
    endDate: string;
    reason: string;
  }): Observable<IApiResponse<ILeaveRequest>> {
    return this.http
      .post<IApiResponse<ILeaveRequest>>(
        'https://localhost:7275/api/LeaveRequests',
        data,
        {
          withCredentials: true,
        }
      )
      .pipe(
        tap((response) => console.log('addLeaveRequest response:', response))
      );
  }

  patchLeaveRequest(params: {
    id: string;
    requestStatus: number;
    reviewerComment?: string;
  }): Observable<IApiResponse<ILeaveRequest[]>> {
    const { id, requestStatus, reviewerComment } = params;
    const body = {
      requestStatus,
      reviewerComment: reviewerComment || '',
    };
    return this.http.patch<IApiResponse<ILeaveRequest[]>>(
      `${this.apiUrl}/review/${id}`,
      body,
      { withCredentials: true }
    );
  }
}
