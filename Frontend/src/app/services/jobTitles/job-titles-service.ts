import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { IJobTitle } from '../../models/entities/ijob-title';
import { IFilteredJobTitlesRequest } from '../../models/requests/ifiltered-job-titles-request';
import { HttpParams } from '@angular/common/http';
import { IFilteredApiResponse } from '../../models/responses/ifiltered-api-response';

@Injectable({
  providedIn: 'root'
})
export class JobTitlesService {
  private baseUrl: string = 'https://localhost:7275/api/jobtitles';

  constructor(private http: HttpClient) {

  }

  getAllJobTitles() : Observable<IJobTitle[]> {
    return this.http.get<IJobTitle[]>(`${this.baseUrl}`);
  }

  getAllJobTitlesFiltered(params :IFilteredJobTitlesRequest) : Observable<IFilteredApiResponse<IJobTitle>> {
    let paramsToSend = new HttpParams();
    
    if (params?.jobTitleName) {
      paramsToSend = paramsToSend.set('jobTitleName', params.jobTitleName);
    }
    
    if (params?.departmentName) {
      paramsToSend = paramsToSend.set('departmentName', params.departmentName);
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

    return this.http.get<IFilteredApiResponse<IJobTitle>>(`${this.baseUrl}/queried`, {params : paramsToSend});
  }
}
