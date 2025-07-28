import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ILeaveRequestType } from '../../models/entities/ileave-request-type';
import { IFilteredJobTitlesRequest } from '../../models/requests/ifiltered-job-titles-request';

@Injectable({
  providedIn: 'root'
})
export class LeaveRequestTypeService {
  private baseUrl: string = 'https://localhost:7275/api/leaverequesttype';

  constructor(private httpClient : HttpClient){

  }

  getAllLeaveRequestTypes() : Observable<ILeaveRequestType[]> {
    return this.httpClient.get<ILeaveRequestType[]>(`${this.baseUrl}`);
  }

  getAllLeaveRequestTypesFiltered() : Observable<IFilteredJobTitlesRequest> {
    return this.httpClient.get<IFilteredJobTitlesRequest>(`${this.baseUrl}/queried`);
  }
}
