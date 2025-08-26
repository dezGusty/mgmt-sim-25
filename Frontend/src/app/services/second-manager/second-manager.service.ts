import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SecondManagerService {
  private apiUrl = `${environment.apiUrl}/SecondManager`;

  constructor(private http: HttpClient) { }

  getSecondManagerInfo() {
    return this.http.get<any>(`${this.apiUrl}/me`, { withCredentials: true });
  }
} 