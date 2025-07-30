import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ILeaveRequestType } from '../../models/entities/ileave-request-type';
import { IFilteredLeaveRequestTypeRequest } from '../../models/requests/ifiltered-leave-request-types-request';
import { IApiResponse } from '../../models/responses/iapi-response';
import { HttpParams } from '@angular/common/http';
import { IFilteredApiResponse } from '../../models/responses/ifiltered-api-response';

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

  getAllLeaveTypesFiltered(params: IFilteredLeaveRequestTypeRequest) : Observable<IApiResponse<IFilteredApiResponse<ILeaveRequestType>>> {
    let paramsToSend = new HttpParams();
    
    if (params?.name) {
      paramsToSend = paramsToSend.set('Description', params.name);
    }
    
    if (params?.params.sortBy) {
      paramsToSend = paramsToSend.set('PagedQueryParams.SortBy', params.params.sortBy);
    }
    
    if (params?.params.sortDescending !== undefined) {
      paramsToSend = paramsToSend.set('PagedQueryParams.SortDescending', params.params.sortDescending.toString());
    }
    
    if (params?.params.page) {
      paramsToSend = paramsToSend.set('PagedQueryParams.Page', params.params.page.toString());
    }
    
    if (params?.params.pageSize) {
      paramsToSend = paramsToSend.set('PagedQueryParams.PageSize', params.params.pageSize.toString());
    }

    return this.httpClient.get<IApiResponse<IFilteredApiResponse<ILeaveRequestType>>>(`${this.baseUrl}/queried`, {params : paramsToSend});
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
}
