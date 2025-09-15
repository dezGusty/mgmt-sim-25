import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PublicHoliday } from '../../../services/public-holidays/public-holidays.service';

export interface CalendarDay {
  date: Date;
  day: number;
  isCurrentMonth: boolean;
  isToday: boolean;
  isWeekend: boolean;
  holidays: PublicHoliday[];
  isSelected?: boolean;
  isDisabled?: boolean;
  customData?: any;
}

export interface CalendarEvent {
  date: Date;
  title: string;
  color?: string;
  data?: any;
}

@Component({
  selector: 'app-shared-calendar',
  templateUrl: './shared-calendar.component.html',
  styleUrl: './shared-calendar.component.css',
  imports: [CommonModule],
  standalone: true
})
export class SharedCalendarComponent implements OnInit, OnChanges {
  @Input() holidays: PublicHoliday[] = [];
  @Input() events: CalendarEvent[] = [];
  @Input() selectedDate: Date | null = null;
  @Input() minDate: Date | null = null;
  @Input() maxDate: Date | null = null;
  @Input() disabledDates: Date[] = [];
  @Input() showWeekNumbers = false;
  @Input() showHolidayNames = true;
  @Input() highlightWeekends = true;
  @Input() compactMode = false;

  @Output() dateClick = new EventEmitter<Date>();
  @Output() holidayClick = new EventEmitter<PublicHoliday>();
  @Output() eventClick = new EventEmitter<CalendarEvent>();
  @Output() monthChange = new EventEmitter<{ month: number; year: number }>();

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
    if (changes['holidays'] || changes['events'] || changes['selectedDate'] || 
        changes['minDate'] || changes['maxDate'] || changes['disabledDates']) {
      this.generateCalendarDays();
    }
  }

  generateCalendarDays() {
    this.calendarDays = [];
    const firstDay = new Date(this.displayYear, this.displayMonth, 1);
    const lastDay = new Date(this.displayYear, this.displayMonth + 1, 0);
    
    // Start from the beginning of the week containing the first day
    const startDate = new Date(firstDay);
    startDate.setDate(startDate.getDate() - startDate.getDay());
    
    // End at the end of the week containing the last day
    const endDate = new Date(lastDay);
    endDate.setDate(endDate.getDate() + (6 - endDate.getDay()));

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    for (let date = new Date(startDate); date <= endDate; date.setDate(date.getDate() + 1)) {
      const currentDate = new Date(date);
      const dayHolidays = this.getHolidaysForDate(currentDate);
      const dayEvents = this.getEventsForDate(currentDate);
      
      this.calendarDays.push({
        date: new Date(currentDate),
        day: currentDate.getDate(),
        isCurrentMonth: currentDate.getMonth() === this.displayMonth,
        isToday: currentDate.getTime() === today.getTime(),
        isWeekend: currentDate.getDay() === 0 || currentDate.getDay() === 6,
        holidays: dayHolidays,
        isSelected: this.selectedDate ? currentDate.getTime() === this.selectedDate.getTime() : false,
        isDisabled: this.isDateDisabled(currentDate),
        customData: { events: dayEvents }
      });
    }
  }

  getHolidaysForDate(date: Date): PublicHoliday[] {
    return this.holidays.filter(holiday => {
      if (holiday.isRecurring) {
        const holidayDate = new Date(holiday.date);
        return holidayDate.getMonth() === date.getMonth() && 
               holidayDate.getDate() === date.getDate();
      } else {
        const holidayDate = new Date(holiday.date);
        return holidayDate.getTime() === date.getTime();
      }
    });
  }

  getEventsForDate(date: Date): CalendarEvent[] {
    return this.events.filter(event => {
      const eventDate = new Date(event.date);
      return eventDate.getTime() === date.getTime();
    });
  }

  isDateDisabled(date: Date): boolean {
    // Check min/max date constraints
    if (this.minDate && date < this.minDate) return true;
    if (this.maxDate && date > this.maxDate) return true;
    
    // Check explicitly disabled dates
    if (this.disabledDates.some(disabledDate => 
      disabledDate.getTime() === date.getTime())) return true;
    
    return false;
  }

  previousMonth() {
    if (this.displayMonth === 0) {
      this.displayMonth = 11;
      this.displayYear--;
    } else {
      this.displayMonth--;
    }
    this.generateCalendarDays();
    this.monthChange.emit({ month: this.displayMonth, year: this.displayYear });
  }

  nextMonth() {
    if (this.displayMonth === 11) {
      this.displayMonth = 0;
      this.displayYear++;
    } else {
      this.displayMonth++;
    }
    this.generateCalendarDays();
    this.monthChange.emit({ month: this.displayMonth, year: this.displayYear });
  }

  goToToday() {
    const today = new Date();
    this.displayMonth = today.getMonth();
    this.displayYear = today.getFullYear();
    this.generateCalendarDays();
    this.monthChange.emit({ month: this.displayMonth, year: this.displayYear });
  }

  onDateClick(day: CalendarDay) {
    if (day.isDisabled) return;
    
    this.dateClick.emit(day.date);
  }

  onHolidayClick(holiday: PublicHoliday, event: Event) {
    event.stopPropagation();
    this.holidayClick.emit(holiday);
  }

  onEventClick(calendarEvent: CalendarEvent, event: Event) {
    event.stopPropagation();
    this.eventClick.emit(calendarEvent);
  }

  getWeekNumber(date: Date): number {
    const onejan = new Date(date.getFullYear(), 0, 1);
    const millisecsInDay = 86400000;
    return Math.ceil((((date.getTime() - onejan.getTime()) / millisecsInDay) + onejan.getDay() + 1) / 7);
  }

  trackByDate(index: number, day: CalendarDay): string {
    return day.date.toISOString();
  }

  get currentMonthYear(): string {
    return `${this.monthNames[this.displayMonth]} ${this.displayYear}`;
  }
}
