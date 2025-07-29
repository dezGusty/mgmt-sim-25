import {
  Component,
  Input,
  OnInit,
  OnChanges,
  SimpleChanges,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ILeaveRequest } from '../../../../models/leave-request';
import { CalendarUtils, CalendarDay } from '../../../../utils/calendar.utils';
import { RequestUtils } from '../../../../utils/request.utils';

@Component({
  selector: 'app-calendar-view',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './calendar-view.html',
  styleUrls: ['./calendar-view.css'],
})
export class CalendarView implements OnInit, OnChanges {
  @Input() requests: ILeaveRequest[] = [];

  currentDate = new Date();
  currentMonth = this.currentDate.getMonth();
  currentYear = this.currentDate.getFullYear();
  calendarDays: CalendarDay[] = [];
  monthNames = CalendarUtils.MONTH_NAMES;
  dayNames = CalendarUtils.DAY_NAMES;

  calendarFilters = {
    pending: true,
    approved: true,
    rejected: false,
  };

  ngOnInit() {
    this.generateCalendar();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['requests']) {
      this.generateCalendar();
    }
  }

  get filteredRequestsForCalendar(): ILeaveRequest[] {
    return RequestUtils.filterRequestsForCalendar(
      this.requests,
      this.calendarFilters
    );
  }

  toggleFilter(status: 'pending' | 'approved') {
    this.calendarFilters[status] = !this.calendarFilters[status];
    this.generateCalendar();
  }

  generateCalendar() {
    this.calendarDays = CalendarUtils.generateCalendarDays(
      this.currentMonth,
      this.currentYear,
      this.filteredRequestsForCalendar
    );
  }

  getLeaveRequestsForDate(date: Date): ILeaveRequest[] {
    return CalendarUtils.getLeaveRequestsForDate(
      date,
      this.filteredRequestsForCalendar
    );
  }

  previousMonth() {
    if (this.currentMonth === 0) {
      this.currentMonth = 11;
      this.currentYear--;
    } else {
      this.currentMonth--;
    }
    this.generateCalendar();
  }

  nextMonth() {
    if (this.currentMonth === 11) {
      this.currentMonth = 0;
      this.currentYear++;
    } else {
      this.currentMonth++;
    }
    this.generateCalendar();
  }

  goToToday() {
    const today = new Date();
    this.currentMonth = today.getMonth();
    this.currentYear = today.getFullYear();
    this.generateCalendar();
  }

  getStatusColor(status: string): string {
    return CalendarUtils.getStatusColor(status);
  }

  isToday(date: Date): boolean {
    return CalendarUtils.isToday(date);
  }
}
