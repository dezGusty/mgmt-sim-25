import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { EmployeeManager } from '../../models/entities/EmployeeManager';

@Injectable({
  providedIn: 'root'
})
export class EmployeeManagerService {
  private baseUrl: string = 'https://localhost:7275/api';

  constructor(private http: HttpClient) {

  }

  getAllRelations(): Observable<EmployeeManager[]> {
    return this.http.get<EmployeeManager[]>(`${this.baseUrl}/employeemanager`);
  }
}
