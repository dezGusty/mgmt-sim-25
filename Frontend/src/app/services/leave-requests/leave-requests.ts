import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class LeaveRequests {
  private apiUrl = 'https://localhost:7275/api/LeaveRequests/by-manager';

  constructor(private http: HttpClient) {}

  fetchByManager(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl, { withCredentials: true }).pipe(
      catchError((err) => {
        console.error('HTTP error fetching leave requests:', err);
        return of([]);
      })
    );
  }
  addLeaveRequest(data: {
    userId: number;
    leaveRequestTypeId: number;
    startDate: string;
    endDate: string;
    reason: string;
  }): Observable<any> {
    return this.http.post('https://localhost:7275/api/LeaveRequest', data);
  }
}
