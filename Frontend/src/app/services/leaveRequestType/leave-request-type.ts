import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LeaveRequestType } from '../../models/entities/LeaveRequestType';

@Injectable({
  providedIn: 'root'
})
export class LeaveRequestTypeService {
  private baseUrl: string = 'https://localhost:7275/api';

  constructor(private httpClient : HttpClient){

  }

  getAllLeaveRequestTypes() : Observable<LeaveRequestType[]> {
    return this.httpClient.get<LeaveRequestType[]>(`${this.baseUrl}/leaverequesttype`);
  }
}
