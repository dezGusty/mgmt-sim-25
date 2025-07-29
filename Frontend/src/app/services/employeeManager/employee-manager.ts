import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IEmployeeManager } from '../../models/entities/iemployee-manager';
import { IApiResponse } from '../../models/responses/iapi-response';

@Injectable({
  providedIn: 'root'
})
export class EmployeeManagerService {
  private baseUrl: string = 'https://localhost:7275/api/employeemanager';

  constructor(private http: HttpClient) {

  }

  addManagersForEmployee(employeeId: number,managersIds: number[]) : Observable<IApiResponse<IEmployeeManager>> {
    return this.http.post<IApiResponse<IEmployeeManager>>(`${this.baseUrl}/addManagersForEmployee`, { employeeId, managersIds} );
  }

  patchManagersForEmployee(employeeId: number,managersIds: number[]) : Observable<IApiResponse<IEmployeeManager>> {
    return this.http.patch<IApiResponse<IEmployeeManager>>(`${this.baseUrl}/patchManagersForEmployee`, { employeeId, managersIds} );
  }
}
