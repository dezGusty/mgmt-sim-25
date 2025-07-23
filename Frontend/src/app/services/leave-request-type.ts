import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';

@Injectable()
export class LeaveRequestTypeService {
  constructor(private http: HttpClient) {}

  getLeaveTypes(): Observable<{ id: number; description: string }[]> {
    return this.http
      .get<any[]>('https://localhost:7275/api/LeaveRequestType', {
        withCredentials: true,
      })
      .pipe(
        map((types) =>
          types.map((t) => ({ id: t.id, description: t.description }))
        )
      );
  }
}
