import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../../models/entities/User';
import { ApiResponse } from '../../models/responses/ApiResponse';

@Injectable({
  providedIn: 'root'
})
export class UsersService {
  private baseUrl: string = 'https://localhost:7275/api';

  constructor(private http: HttpClient) {}

  getAllUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.baseUrl}/users`);
  }
}