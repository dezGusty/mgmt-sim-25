import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable()
export class EmployeeService {
  constructor(private http: HttpClient) {}

  getEmployees(): Observable<{ id: number; name: string }[]> {
    return this.http
      .get<any>(
        `${environment.apiUrl}/EmployeeManager/employeesByManager`,
        {
          withCredentials: true,
        }
      )
      .pipe(
        map((response) => {
          if (response.success && Array.isArray(response.data)) {
            return response.data.map(
              (u: { id: number; firstName: string; lastName: string }) => ({
                id: u.id,
                name: u.firstName + ' ' + u.lastName,
              })
            );
          }
          return [];
        })
      );
  }
}
