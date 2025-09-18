import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { IApiResponse } from '../../models/responses/iapi-response';

export interface WeekendConfiguration {
  weekendDays: string[];
  weekendDaysCount: number;
  isValid: boolean;
  lastUpdated: string;
  availableDays: string[];
}

export interface UpdateWeekendConfigurationRequest {
  weekendDays: string[];
  weekendDaysCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class WeekendConfigurationService {
  private baseUrl = `${environment.apiUrl}/hr`;

  constructor(private http: HttpClient) {}

  /**
   * Get the current weekend configuration
   */
  getWeekendConfiguration(): Observable<IApiResponse<WeekendConfiguration>> {
    return this.http.get<IApiResponse<WeekendConfiguration>>(`${this.baseUrl}/weekend-configuration`, {
      withCredentials: true
    });
  }

  /**
   * Update the weekend configuration
   */
  updateWeekendConfiguration(config: UpdateWeekendConfigurationRequest): Observable<IApiResponse<WeekendConfiguration>> {
    return this.http.put<IApiResponse<WeekendConfiguration>>(`${this.baseUrl}/weekend-configuration`, config, {
      withCredentials: true
    });
  }

  /**
   * Get available day names
   */
  getAvailableDays(): string[] {
    return ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];
  }

  /**
   * Validate weekend configuration
   */
  validateConfiguration(weekendDays: string[], weekendDaysCount: number): { isValid: boolean; errors: string[] } {
    const errors: string[] = [];
    
    if (weekendDays.length !== weekendDaysCount) {
      errors.push('Weekend days count must match the number of selected days');
    }
    
    if (weekendDays.length === 0) {
      errors.push('At least one weekend day must be selected');
    }
    
    if (weekendDays.length > 7) {
      errors.push('Cannot have more than 7 weekend days');
    }
    
    const availableDays = this.getAvailableDays();
    const invalidDays = weekendDays.filter(day => !availableDays.includes(day));
    if (invalidDays.length > 0) {
      errors.push(`Invalid day names: ${invalidDays.join(', ')}`);
    }
    
    const duplicateDays = weekendDays.filter((day, index) => weekendDays.indexOf(day) !== index);
    if (duplicateDays.length > 0) {
      errors.push(`Duplicate days: ${duplicateDays.join(', ')}`);
    }
    
    return {
      isValid: errors.length === 0,
      errors
    };
  }
}
