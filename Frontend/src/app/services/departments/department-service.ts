import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IDepartment } from '../../models/entities/idepartment';
import { IFilteredDepartmentsRequest } from '../../models/requests/ifiltered-departments-request';
import { IFilteredApiResponse } from '../../models/responses/ifiltered-api-response';
import { HttpParams } from '@angular/common/http';
import { IApiResponse } from '../../models/responses/iapi-response';

@Injectable({
  providedIn: 'root'
})
export class DepartmentService {
  private readonly apiUrl = 'https://localhost:7275/api/departments';

  constructor(private http: HttpClient) { }

  getAllDepartments(): Observable<IApiResponse<IDepartment[]>> {
    return this.http.get<IApiResponse<IDepartment[]>>(this.apiUrl);
  }

  addDepartment(department : IDepartment) : Observable<IApiResponse<IDepartment>> {
    return this.http.post<IApiResponse<IDepartment>>(this.apiUrl, department)
  }

  getAllDepartmentsFiltered(params: IFilteredDepartmentsRequest): Observable<IApiResponse<IFilteredApiResponse<IDepartment>>> {
    let paramsToSend = new HttpParams();
    
    if (params?.name) {
      paramsToSend = paramsToSend.set('name', params.name);
    }

    if (params?.includeDeleted) {
      paramsToSend = paramsToSend.set('includeDeleted', params.includeDeleted);
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

    return this.http.get<IApiResponse<IFilteredApiResponse<IDepartment>>>(`${this.apiUrl}/queried`, {params : paramsToSend});
  }

  getDepartmentById(id: number): Observable<IApiResponse<IDepartment>> {
    return this.http.get<IApiResponse<IDepartment>>(`${this.apiUrl}/${id}`);
  }

  createDepartment(department: IDepartment): Observable<IApiResponse<IDepartment>> {
    return this.http.post<IApiResponse<IDepartment>>(this.apiUrl, department);
  }

  updateDepartment(id: number, department: IDepartment): Observable<IApiResponse<IDepartment>> {
    return this.http.patch<IApiResponse<IDepartment>>(`${this.apiUrl}/${id}`, department);
  }

  deleteDepartment(id: number): Observable<IApiResponse<boolean>> {
    return this.http.delete<IApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }

  restoreDepartment(id: number): Observable<IApiResponse<IDepartment>> {
    return this.http.patch<IApiResponse<IDepartment>>(`${this.apiUrl}/restore/${id}`, {})
  }
}
