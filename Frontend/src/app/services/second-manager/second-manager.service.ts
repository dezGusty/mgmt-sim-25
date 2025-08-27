import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ISecondManagerResponse } from '../../models/entities/isecond-manager';

@Injectable({
  providedIn: 'root'
})
export class SecondManagerService {
  private apiUrl = `${environment.apiUrl}/SecondManager`;

  constructor(private http: HttpClient) { }

  getSecondManagerInfo() {
    return this.http.get<any>(`${this.apiUrl}/me`, { withCredentials: true });
  }

  getActiveSecondManagers(): Observable<ISecondManagerResponse[]> {
    return this.http.get<ISecondManagerResponse[]>(`${this.apiUrl}/active`, { withCredentials: true });
  }

  getAllSecondManagers(): Observable<ISecondManagerResponse[]> {
    return this.http.get<ISecondManagerResponse[]>(`${this.apiUrl}`, { withCredentials: true });
  }

  getSecondManagersByReplacedManagerId(replacedManagerId: number): Observable<ISecondManagerResponse[]> {
    return this.http.get<ISecondManagerResponse[]>(`${this.apiUrl}/by-replaced-manager/${replacedManagerId}`, { withCredentials: true });
  }
} 