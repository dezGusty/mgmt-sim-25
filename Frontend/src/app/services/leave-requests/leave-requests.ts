import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, of, tap, switchMap } from 'rxjs';
import { IApiResponse } from '../../models/responses/iapi-response';
import { ILeaveRequest } from '../../models/leave-request';
import { CreateLeaveRequestResponse } from '../../models/create-leave-request-response';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class LeaveRequests {
  private apiUrl = `${environment.apiUrl}/LeaveRequests`;

  constructor(private http: HttpClient) {}

  fetchByManager(name?: string): Observable<IApiResponse<any[]>> {
    const params: any = {};
    if (name && name.trim().length > 0) {
      params.name = name;
    }
    return this.http
      .get<IApiResponse<any[]>>(`${this.apiUrl}/by-manager`, {
        params,
        withCredentials: true,
      })
      .pipe(
        catchError((err: HttpErrorResponse) => {
          const base = { data: [] as any[], success: false, timestamp: new Date() };

          if (err.status === 404) {
            return of({ ...base, message: 'No leave requests found for your team.' } satisfies IApiResponse<any[]>);
          }

          if (err.status === 401 || err.status === 403) {
            return of({ ...base, message: 'You are not authorized to view these requests.' } satisfies IApiResponse<any[]>);
          }

          return of({ ...base, message: 'Failed to load leave requests. Please try again later.' } satisfies IApiResponse<any[]>);
        })
      );
  }

  fetchByManagerPaginated(status: string = 'ALL', pageSize: number = 10, pageNumber: number = 1): Observable<IApiResponse<{items: any[], totalCount: number, totalPages: number}>> {
    return this.http
      .get<IApiResponse<{items: any[], totalCount: number, totalPages: number}>>(`${this.apiUrl}/filtered`, {
        params: {
          status,
          pageSize: pageSize.toString(),
          pageNumber: pageNumber.toString()
        },
        withCredentials: true,
      })
      .pipe(
        catchError((err: HttpErrorResponse) => {
          const base = { data: {items: [] as any[], totalCount: 0, totalPages: 0}, success: false, timestamp: new Date() };

          if (err.status === 404) {
            return of({ ...base, message: 'No leave requests found for your team.' });
          }

          if (err.status === 401 || err.status === 403) {
            return of({ ...base, message: 'You are not authorized to view these requests.' });
          }

          return of({ ...base, message: 'Failed to load leave requests. Please try again later.' });
        })
      );
  }

  addLeaveRequest(data: {
    userId: number;
    leaveRequestTypeId: number;
    startDate: string;
    endDate: string;
    reason: string;
    reviewerId?: number;
  }): Observable<IApiResponse<CreateLeaveRequestResponse>> {
    return this.http
      .post<IApiResponse<CreateLeaveRequestResponse>>(this.apiUrl, data, {
        withCredentials: true,
      })
      .pipe(
        tap((response) => console.log('addLeaveRequest response:', response)),
        switchMap((response) => {
          if (response.data && response.data.id) {
            return this.patchLeaveRequest({
              id: response.data.id.toString(),
              requestStatus: 4,
              reviewerComment: 'Auto-approved by manager',
            }).pipe(
              tap((patchResponse) =>
                console.log('Request auto-approved:', patchResponse)
              ),
              switchMap(() => {
                const updatedResponse = {
                  ...response,
                  data: {
                    ...response.data,
                    requestStatus: 4,
                    reviewerComment: 'Auto-approved by manager',
                  },
                };
                return of(updatedResponse);
              })
            );
          }
          return of(response);
        })
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
