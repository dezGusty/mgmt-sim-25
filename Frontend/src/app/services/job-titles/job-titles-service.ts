import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { IJobTitle } from '../../models/entities/ijob-title';
import { IFilteredJobTitlesRequest } from '../../models/requests/ifiltered-job-titles-request';
import { HttpParams } from '@angular/common/http';
import { IFilteredApiResponse } from '../../models/responses/ifiltered-api-response';
import { IApiResponse } from '../../models/responses/iapi-response';

@Injectable({
  providedIn: 'root'
})
export class JobTitlesService {
  private baseUrl: string = 'https://localhost:7275/api/jobtitles';

  constructor(private http: HttpClient) {

  }

  getAllJobTitles() : Observable<IApiResponse<IJobTitle[]>> {
    return this.http.get<IApiResponse<IJobTitle[]>>(`${this.baseUrl}`);
  }

  addJobTitle(jobTitle: IJobTitle): Observable<IApiResponse<IJobTitle>> {
    return this.http.post<IApiResponse<IJobTitle>>(`${this.baseUrl}`, jobTitle);
  }

  updateJobTitle(jobTitle: IJobTitle): Observable<IApiResponse<IJobTitle>> {
    return this.http.patch<IApiResponse<IJobTitle>>(`${this.baseUrl}/${jobTitle.id}`, jobTitle);
  }

  deleteJobTitle(id: number): Observable<IApiResponse<void>> {
    return this.http.delete<IApiResponse<void>>(`${this.baseUrl}/${id}`);
  }

  getJobTitleById(id: number): Observable<IApiResponse<IJobTitle>> {
    return this.http.get<IApiResponse<IJobTitle>>(`${this.baseUrl}/${id}`);
  }

  getAllJobTitlesFiltered(params :IFilteredJobTitlesRequest) : Observable<IApiResponse<IFilteredApiResponse<IJobTitle>>> {
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

    return this.http.get<IApiResponse<IFilteredApiResponse<IJobTitle>>>(`${this.baseUrl}/queried`, {params : paramsToSend});
  }
}
