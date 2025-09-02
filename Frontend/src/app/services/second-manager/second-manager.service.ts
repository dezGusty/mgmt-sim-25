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

  createSecondManager(request: { secondManagerEmployeeId: number; replacedManagerId: number; startDate: string; endDate: string }): Observable<ISecondManagerResponse> {
    return this.http.post<ISecondManagerResponse>(`${this.apiUrl}`, request, { withCredentials: true });
  }

    deleteSecondManager(secondManagerEmployeeId: number, replacedManagerId: number, startDate: Date | string): Observable<void> {
    const dateObj = typeof startDate === 'string' ? new Date(startDate) : startDate;
    
    const formattedDate = encodeURIComponent(this.formatDateForUrl(dateObj));
    
    console.log('Deleting second manager with URL:', `${this.apiUrl}/${secondManagerEmployeeId}/${replacedManagerId}/${formattedDate}`);
    return this.http.delete<void>(`${this.apiUrl}/${secondManagerEmployeeId}/${replacedManagerId}/${formattedDate}`, { withCredentials: true });
  }

  private formatDateForUrl(date: Date): string {
    const year = date.getFullYear();
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const day = date.getDate().toString().padStart(2, '0');
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    const seconds = date.getSeconds().toString().padStart(2, '0');
    const milliseconds = date.getMilliseconds().toString().padStart(3, '0');

    return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}.${milliseconds}0000`;
  }
} 