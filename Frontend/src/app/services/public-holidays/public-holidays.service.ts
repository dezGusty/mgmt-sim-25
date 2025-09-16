import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { map, tap, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface PublicHoliday {
  id?: number;
  name: string;
  date: string; // Format: YYYY-MM-DD
  isRecurring: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class PublicHolidaysService {
  private baseUrl = `${environment.apiUrl}/hr`;
  private holidaysCache = new Map<number, PublicHoliday[]>();
  private holidaysSubject = new BehaviorSubject<PublicHoliday[]>([]);
  
  constructor(private http: HttpClient) {}

  /**
   * Get public holidays for a specific year
   */
  getPublicHolidays(year: number): Observable<PublicHoliday[]> {
    // Check cache first
    if (this.holidaysCache.has(year)) {
      const cached = this.holidaysCache.get(year)!;
      this.holidaysSubject.next(cached);
      return of(cached);
    }

    const params = new HttpParams().set('year', year.toString());
    
    return this.http.get<any>(`${this.baseUrl}/public-holidays`, { params }).pipe(
      map(response => {
        // Handle the API response structure
        const holidays = response?.Data || response?.data || response;
        return Array.isArray(holidays) ? holidays : [];
      }),
      tap(holidays => {
        // Cache the results
        this.holidaysCache.set(year, holidays);
        this.holidaysSubject.next(holidays);
      }),
      catchError(error => {
        console.error('Error fetching public holidays:', error);
        return of([]);
      })
    );
  }

  /**
   * Get holidays for multiple years
   */
  getPublicHolidaysForYears(years: number[]): Observable<PublicHoliday[]> {
    const requests = years.map(year => this.getPublicHolidays(year));
    
    return new Observable(observer => {
      const allHolidays: PublicHoliday[] = [];
      let completed = 0;

      requests.forEach(request => {
        request.subscribe({
          next: holidays => {
            allHolidays.push(...holidays);
            completed++;
            
            if (completed === requests.length) {
              // Remove duplicates and sort by date
              const uniqueHolidays = this.removeDuplicateHolidays(allHolidays);
              observer.next(uniqueHolidays);
              observer.complete();
            }
          },
          error: error => {
            console.error('Error in getPublicHolidaysForYears:', error);
            completed++;
            
            if (completed === requests.length) {
              const uniqueHolidays = this.removeDuplicateHolidays(allHolidays);
              observer.next(uniqueHolidays);
              observer.complete();
            }
          }
        });
      });
    });
  }

  /**
   * Check if a specific date is a public holiday
   */
  isPublicHoliday(date: Date, holidays: PublicHoliday[]): boolean {
    const dateString = this.formatDateToString(date);
    return holidays.some(holiday => {
      if (holiday.isRecurring) {
        // For recurring holidays, only compare month and day
        const holidayDate = new Date(holiday.date);
        return holidayDate.getMonth() === date.getMonth() && 
               holidayDate.getDate() === date.getDate();
      } else {
        // For non-recurring holidays, exact date match
        return holiday.date === dateString;
      }
    });
  }

  /**
   * Get holiday information for a specific date
   */
  getHolidayForDate(date: Date, holidays: PublicHoliday[]): PublicHoliday | null {
    const dateString = this.formatDateToString(date);
    
    return holidays.find(holiday => {
      if (holiday.isRecurring) {
        // For recurring holidays, only compare month and day
        const holidayDate = new Date(holiday.date);
        return holidayDate.getMonth() === date.getMonth() && 
               holidayDate.getDate() === date.getDate();
      } else {
        // For non-recurring holidays, exact date match
        return holiday.date === dateString;
      }
    }) || null;
  }

  /**
   * Get holidays within a date range
   */
  getHolidaysInRange(startDate: Date, endDate: Date): Observable<PublicHoliday[]> {
    const startYear = startDate.getFullYear();
    const endYear = endDate.getFullYear();
    
    const years = [];
    for (let year = startYear; year <= endYear; year++) {
      years.push(year);
    }

    return this.getPublicHolidaysForYears(years).pipe(
      map(holidays => {
        return holidays.filter(holiday => {
          const holidayDate = new Date(holiday.date);
          if (holiday.isRecurring) {
            // For recurring holidays, check if they fall within the range for any year
            for (let year = startYear; year <= endYear; year++) {
              const recurringDate = new Date(year, holidayDate.getMonth(), holidayDate.getDate());
              if (recurringDate >= startDate && recurringDate <= endDate) {
                return true;
              }
            }
            return false;
          } else {
            // For non-recurring holidays, simple date range check
            return holidayDate >= startDate && holidayDate <= endDate;
          }
        });
      })
    );
  }

  /**
   * Clear cache for a specific year or all years
   */
  clearCache(year?: number): void {
    if (year) {
      this.holidaysCache.delete(year);
    } else {
      this.holidaysCache.clear();
    }
  }

  /**
   * Get observable for holidays updates
   */
  getHolidaysObservable(): Observable<PublicHoliday[]> {
    return this.holidaysSubject.asObservable();
  }

  private formatDateToString(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private removeDuplicateHolidays(holidays: PublicHoliday[]): PublicHoliday[] {
    const seen = new Set<string>();
    return holidays.filter(holiday => {
      const key = `${holiday.name}-${holiday.date}-${holiday.isRecurring}`;
      if (seen.has(key)) {
        return false;
      }
      seen.add(key);
      return true;
    }).sort((a, b) => a.date.localeCompare(b.date));
  }
}
