import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ILeaveRequestType } from '../../models/entities/ileave-request-type';
import { IFilteredJobTitlesRequest } from '../../models/requests/ifiltered-job-titles-request';
import { IApiResponse } from '../../models/responses/iapi-response';

@Injectable({
  providedIn: 'root'
})
export class LeaveRequestTypeService {
  private baseUrl: string = 'https://localhost:7275/api/leaverequesttype';

  constructor(private httpClient : HttpClient){

  }

  getAllLeaveRequestTypes() : Observable<IApiResponse<ILeaveRequestType[]>> {
    return this.httpClient.get<IApiResponse<ILeaveRequestType[]>>(`${this.baseUrl}`);
  }

  getLeaveRequestTypeById(id: number): Observable<IApiResponse<ILeaveRequestType>> {
    return this.httpClient.get<IApiResponse<ILeaveRequestType>>(`${this.baseUrl}/${id}`);
  }

  addLeaveRequestType(leaveRequestType: ILeaveRequestType): Observable<IApiResponse<ILeaveRequestType>> {
    return this.httpClient.post<IApiResponse<ILeaveRequestType>>(`${this.baseUrl}`, leaveRequestType);
  }

  updateLeaveRequestType(leaveRequestType: ILeaveRequestType): Observable<IApiResponse<ILeaveRequestType>> {
    return this.httpClient.patch<IApiResponse<ILeaveRequestType>>(`${this.baseUrl}/${leaveRequestType.id}`, leaveRequestType);
  }

  deleteLeaveRequestType(id: number): Observable<IApiResponse<boolean>> {
    return this.httpClient.delete<IApiResponse<boolean>>(`${this.baseUrl}/${id}`);
  }

  getAllLeaveRequestTypesFiltered() : Observable<IApiResponse<IFilteredJobTitlesRequest>> {
    return this.httpClient.get<IApiResponse<IFilteredJobTitlesRequest>>(`${this.baseUrl}/queried`);
  }
}
