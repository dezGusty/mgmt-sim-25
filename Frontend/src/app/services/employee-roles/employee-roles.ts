import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IApiResponse } from '../../models/responses/iapi-response';
import { IEmployeeRole } from '../../models/entities/iemployee-role';

@Injectable({
  providedIn: 'root'
})
export class EmployeeRolesService {
  private baseUrl = 'https://localhost:7275/api/employeeRole'

  constructor(private httpClient: HttpClient) {

  }

  getAllEmployeeRoles() : Observable<IApiResponse<IEmployeeRole[]>> {
    return this.httpClient.get<IApiResponse<IEmployeeRole[]>>(this.baseUrl);
  }
}
