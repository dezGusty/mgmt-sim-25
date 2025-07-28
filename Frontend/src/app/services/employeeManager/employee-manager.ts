import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IEmployeeManager } from '../../models/entities/iemployee-manager';

@Injectable({
  providedIn: 'root'
})
export class EmployeeManagerService {
  private baseUrl: string = 'https://localhost:7275/api';

  constructor(private http: HttpClient) {

  }

  getAllRelations(): Observable<IEmployeeManager[]> {
    return this.http.get<IEmployeeManager[]>(`${this.baseUrl}/employeemanager`);
  }
}
