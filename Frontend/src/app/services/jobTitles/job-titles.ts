import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { JobTitle } from '../../models/entities/JobTitle';

@Injectable({
  providedIn: 'root'
})
export class JobTitlesService {
  private baseUrl: string = 'https://localhost:7275/api';

  constructor(private http: HttpClient) {

  }

  getAllJobTitles() : Observable<JobTitle[]> {
    return this.http.get<JobTitle[]>(`${this.baseUrl}/jobtitles`);
  }
}
