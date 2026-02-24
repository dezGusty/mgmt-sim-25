import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IApiResponse } from '../../models/responses/iapi-response';
import { IEmployeeRole } from '../../models/entities/iemployee-role';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class EmployeeRolesService {
  private baseUrl = `${environment.apiUrl}/employeeRole`

  constructor(private httpClient: HttpClient) {

  }

  getAllEmployeeRoles() : Observable<IApiResponse<IEmployeeRole[]>> {
    return this.httpClient.get<IApiResponse<IEmployeeRole[]>>(this.baseUrl);
  }
}
