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

export interface ImportedPublicHoliday {
  name: string;
  date: string;
  recurring: boolean | string | number;
  isValid?: boolean;
  errors?: string[];
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

  validateImportedHoliday(holiday: ImportedPublicHoliday): { isValid: boolean; errors: string[] } {
    const errors: string[] = [];

    if (!holiday.name || typeof holiday.name !== 'string' || holiday.name.trim().length === 0) {
      errors.push('Name is required');
    }

    if (!holiday.date || typeof holiday.date !== 'string' || holiday.date.trim().length === 0) {
      errors.push('Date is required');
    } else {
      const date = new Date(holiday.date);
      if (isNaN(date.getTime())) {
        errors.push('Invalid date format. Use YYYY-MM-DD');
      }
    }

    const recurringValue = holiday.recurring;
    if (recurringValue !== undefined && 
        recurringValue !== null && 
        recurringValue !== true && 
        recurringValue !== false && 
        recurringValue !== 'true' && 
        recurringValue !== 'false' && 
        recurringValue !== '1' && 
        recurringValue !== '0' && 
        recurringValue !== 1 && 
        recurringValue !== 0) {
      errors.push('Recurring must be true/false, 1/0, or yes/no');
    }

    return { isValid: errors.length === 0, errors };
  }

  convertImportedToPublicHoliday(imported: ImportedPublicHoliday): PublicHoliday {
    let isRecurring = false;
    const recurringValue = imported.recurring;
    
    if (recurringValue === true || recurringValue === 'true' || recurringValue === '1' || String(recurringValue) === '1') {
      isRecurring = true;
    }

    return {
      name: imported.name.trim(),
      date: imported.date,
      isRecurring
    };
  }
}
