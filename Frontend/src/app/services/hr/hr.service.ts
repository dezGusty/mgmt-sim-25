import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface IHrUserDto {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  roles: string[];
  jobTitleId: number;
  jobTitleName: string;
  departmentId: number;
  departmentName: string;
  isActive: boolean;
  dateOfEmployment: string;
  vacation: number;

  totalLeaveDays: number;
  usedLeaveDays: number;
  remainingLeaveDays: number;
}

export interface IPagedResponse<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface PublicHoliday {
  id?: number;
  name: string;
  date: string;
  isRecurring: boolean;
}

@Injectable({ providedIn: 'root' })
export class HrService {
  private baseUrl = `${environment.apiUrl}/hr`;

  constructor(private http: HttpClient) {}

  getUsers(year: number, page: number, pageSize: number, department?: string): Observable<IPagedResponse<IHrUserDto>> {
    let params = new HttpParams()
      .set('year', year.toString())
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (department) {
      params = params.set('department', department);
    }

    return this.http.get<any>(`${this.baseUrl}/users`, { params }).pipe(

    ) as Observable<IPagedResponse<IHrUserDto>>;
  }

  adjustVacation(userId: number, days: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/users/${userId}/vacation`, { id: userId, days });
  }

  getPublicHolidays(year: number): Observable<PublicHoliday[]> {
    const params = new HttpParams().set('year', year.toString());
    return this.http.get<PublicHoliday[]>(`${this.baseUrl}/public-holidays`, { params });
  }

  createPublicHoliday(holiday: Omit<PublicHoliday, 'id'>): Observable<PublicHoliday> {
    return this.http.post<PublicHoliday>(`${this.baseUrl}/public-holidays`, holiday);
  }

  updatePublicHoliday(holiday: PublicHoliday): Observable<PublicHoliday> {
    return this.http.put<PublicHoliday>(`${this.baseUrl}/public-holidays/${holiday.id}`, holiday);
  }

  deletePublicHoliday(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/public-holidays/${id}`);
  }
}
