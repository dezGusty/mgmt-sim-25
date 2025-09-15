import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PublicHoliday } from '../../../services/public-holidays/public-holidays.service';

interface CalendarDay {
  date: Date;
  day: number;
  isCurrentMonth: boolean;
  isToday: boolean;
  holidays: PublicHoliday[];
}

@Component({
  selector: 'app-holiday-calendar',
  templateUrl: './holiday-calendar.component.html',
  styleUrl: './holiday-calendar.component.css',
  imports: [CommonModule]
})
export class HolidayCalendarComponent implements OnInit, OnChanges {
  @Input() holidays: PublicHoliday[] = [];
  @Output() dateClick = new EventEmitter<Date>();
  @Output() holidayClick = new EventEmitter<PublicHoliday>();

  currentDate = new Date();
  displayMonth = new Date().getMonth();
  displayYear = new Date().getFullYear();
  
  calendarDays: CalendarDay[] = [];
  weekDays = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
  monthNames = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];

  ngOnInit() {
    this.generateCalendarDays();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['holidays']) {
      this.generateCalendarDays();
    }
  }

  generateCalendarDays() {
    this.calendarDays = [];
    const firstDay = new Date(this.displayYear, this.displayMonth, 1);
    const lastDay = new Date(this.displayYear, this.displayMonth + 1, 0);
    const startDate = new Date(firstDay);
    const endDate = new Date(lastDay);

    // Start from the beginning of the week containing the first day of the month
    startDate.setDate(startDate.getDate() - startDate.getDay());
    
    // End at the end of the week containing the last day of the month
    endDate.setDate(endDate.getDate() + (6 - endDate.getDay()));

    const currentDate = new Date(startDate);
    while (currentDate <= endDate) {
      const dayHolidays = this.getHolidaysForDate(currentDate);
      
      this.calendarDays.push({
        date: new Date(currentDate),
        day: currentDate.getDate(),
        isCurrentMonth: currentDate.getMonth() === this.displayMonth,
        isToday: this.isSameDay(currentDate, new Date()),
        holidays: dayHolidays
      });

      currentDate.setDate(currentDate.getDate() + 1);
    }
  }

  getHolidaysForDate(date: Date): PublicHoliday[] {
    return this.holidays.filter(holiday => {
      const holidayDate = new Date(holiday.date);
      
      if (holiday.isRecurring) {
        // For recurring holidays, match month and day regardless of year
        return holidayDate.getMonth() === date.getMonth() && 
               holidayDate.getDate() === date.getDate();
      } else {
        // For non-recurring holidays, match exact date
        return this.isSameDay(holidayDate, date);
      }
    });
  }

  isSameDay(date1: Date, date2: Date): boolean {
    return date1.getFullYear() === date2.getFullYear() &&
           date1.getMonth() === date2.getMonth() &&
           date1.getDate() === date2.getDate();
  }

  previousMonth() {
    if (this.displayMonth === 0) {
      this.displayMonth = 11;
      this.displayYear--;
    } else {
      this.displayMonth--;
    }
    this.generateCalendarDays();
  }

  nextMonth() {
    if (this.displayMonth === 11) {
      this.displayMonth = 0;
      this.displayYear++;
    } else {
      this.displayMonth++;
    }
    this.generateCalendarDays();
  }

  goToToday() {
    const today = new Date();
    this.displayMonth = today.getMonth();
    this.displayYear = today.getFullYear();
    this.generateCalendarDays();
  }

  onDateClick(day: CalendarDay) {
    if (day.holidays.length > 0) {
      // If there are holidays on this date, emit the first one for editing
      this.holidayClick.emit(day.holidays[0]);
    } else {
      // If no holidays, emit date for adding new holiday
      this.dateClick.emit(day.date);
    }
  }

  onHolidayClick(holiday: PublicHoliday, event: Event) {
    event.stopPropagation();
    this.holidayClick.emit(holiday);
  }

  get currentMonthName(): string {
    return this.monthNames[this.displayMonth];
  }

  trackByDate(index: number, day: CalendarDay): string {
    return day.date.toISOString();
  }

  getAdditionalHolidaysTitle(holidays: PublicHoliday[]): string {
    return holidays.slice(2).map(h => h.name).join(', ');
  }
}