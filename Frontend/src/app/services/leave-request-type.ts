import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';

interface LeaveRequestTypeResponse {
  data: { id: number; description: string; additionalDetails: string }[];
}

@Injectable()
export class LeaveRequestTypeService {
  constructor(private http: HttpClient) {}

  getLeaveTypes(): Observable<
    { id: number; description: string; additionalDetails: string }[]
  > {
    return this.http
      .get<LeaveRequestTypeResponse>(
        'https://localhost:7275/api/LeaveRequestType',
        {
          withCredentials: true,
        }
      )
      .pipe(
        map((response) => {
          if (response.data && Array.isArray(response.data)) {
            return response.data.map((t) => ({
              id: t.id,
              description: t.description,
              additionalDetails: t.additionalDetails,
            }));
          }
          console.error('Unexpected response format:', response);
          return [];
        })
      );
  }
}
