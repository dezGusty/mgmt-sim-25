import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of, tap, switchMap } from 'rxjs';
import { IApiResponse } from '../../models/responses/iapi-response';
import { IUser } from '../../models/entities/iuser';
import { ILeaveRequest } from '../../models/leave-request';
import { CreateLeaveRequestResponse } from '../../models/create-leave-request-response';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class LeaveRequests {
  private apiUrl = `${environment.apiUrl}/LeaveRequests`;

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
