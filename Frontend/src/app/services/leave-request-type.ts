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

  getLeaveTypes(): Observable<{ id: number; description: string }[]> {
    return this.http
      .get<any[]>('https://localhost:7275/api/LeaveRequestType', {
        withCredentials: true,
      })
      .pipe(
        map((types) =>
          types.map((t) => ({ id: t.id, description: t.description }))
        )
      );
  }

  postLeaveRequestType(lrt: ILeaveRequestType) : Observable<IApiResponse<ILeaveRequestType>> {
    return this.http.post<IApiResponse<ILeaveRequestType>>(this.baseUrl, lrt);
  }
}
