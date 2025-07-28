import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { IApiResponse } from '../../models/responses/iapi-response';

@Injectable({
  providedIn: 'root',
})
export class LeaveRequests {
  private apiUrl = 'https://localhost:7275/api/LeaveRequests';

  constructor(private http: HttpClient) {}

  fetchByManager(): Observable<any[]> {
    return this.http
      .get<any[]>(`${this.apiUrl}/by-manager`, { withCredentials: true })
      .pipe(
        catchError((err) => {
          console.error('HTTP error fetching leave requests:', err);
          return of([]);
        })
      );
  }

  addLeaveRequest(data: {
    userId: number;
    leaveRequestTypeId: number;
    startDate: string;
    endDate: string;
    reason: string;
  }): Observable<any> {
    return this.http.post('https://localhost:7275/api/LeaveRequests', data, {
      withCredentials: true,
    });
  }

  patchLeaveRequest(params: {
    id: string;
    requestStatus: number;
    reviewerComment?: string;
  }): Observable<any> {
    const { id, requestStatus, reviewerComment } = params;
    const body = {
      requestStatus,
      reviewerComment: reviewerComment || '',
    };
    return this.http
      .patch(`${this.apiUrl}/review/${id}`, body, { withCredentials: true })
      .pipe(
        catchError((err) => {
          console.error('HTTP error patching leave request:', err);
          return of(null);
        })
      );
  }
}
