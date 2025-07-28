import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IDepartment } from '../../models/entities/idepartment';
import { IFilteredDepartmentsRequest } from '../../models/requests/ifiltered-departments-request';
import { IFilteredApiResponse } from '../../models/responses/ifiltered-api-response';
import { HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class DepartmentService {
  private readonly apiUrl = 'https://localhost:7275/api/departments';

  constructor(private http: HttpClient) { }

  getAllDepartments(): Observable<IDepartment[]> {
    return this.http.get<IDepartment[]>(this.apiUrl);
  }

  getAllDepartmentsFiltered(params: IFilteredDepartmentsRequest): Observable<IFilteredApiResponse<IDepartment>> {
    let paramsToSend = new HttpParams();
    
    if (params?.name) {
      paramsToSend = paramsToSend.set('lastName', params.name);
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

    return this.http.get<IFilteredApiResponse<IDepartment>>(`${this.apiUrl}/queried`, {params : paramsToSend});
  }

  getDepartmentById(id: number): Observable<IDepartment> {
    return this.http.get<IDepartment>(`${this.apiUrl}/${id}`);
  }

  createDepartment(department: IDepartment): Observable<IDepartment> {
    return this.http.post<IDepartment>(this.apiUrl, department);
  }

  updateDepartment(id: number, department: IDepartment): Observable<IDepartment> {
    return this.http.put<IDepartment>(`${this.apiUrl}/${id}`, department);
  }

  deleteDepartment(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
