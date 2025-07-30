import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ILeaveRequestType } from '../models/entities/ileave-request-type';
import { IApiResponse } from '../models/responses/iapi-response';

@Injectable({
  providedIn : "root"
})
export class LeaveRequestTypeService {
  private baseUrl: string = "https://localhost:7275/api/leaveRequestType";

  constructor(private http: HttpClient) {}

  getLeaveTypes(): Observable<
    { id: number; description: string; additionalDetails: string }[]
  > {
    return this.http
      .get<LeaveRequestTypeResponse>(
        'https://localhost:7275/api/LeaveRequestType',
        {
          withCredentials: true,
        }
      )
      .pipe(
        map((response) => {
          if (response.data && Array.isArray(response.data)) {
            return response.data.map((t) => ({
              id: t.id,
              description: t.description,
              additionalDetails: t.additionalDetails,
            }));
          }
          console.error('Unexpected response format:', response);
          return [];
        })
      );
  }

  postLeaveRequestType(lrt: ILeaveRequestType) : Observable<IApiResponse<ILeaveRequestType>> {
    return this.http.post<IApiResponse<ILeaveRequestType>>(this.baseUrl, lrt);
  }
}
