import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';

@Injectable()
export class EmployeeService {
  constructor(private http: HttpClient) {}

  getEmployees(): Observable<{ id: number; name: string }[]> {
    return this.http
      .get<any[]>('https://localhost:7275/employeesByManager', {
        withCredentials: true,
      })
      .pipe(
        map((users) =>
          users.map((u) => ({
            id: u.id,
            name: u.firstName + ' ' + u.lastName,
          }))
        )
      );
  }
}
