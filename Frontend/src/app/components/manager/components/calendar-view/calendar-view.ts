import {
  Component,
  Input,
  OnInit,
  OnChanges,
  SimpleChanges,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ILeaveRequest } from '../../../../models/leave-request';

interface CalendarDay {
  date: Date;
  dayNumber: number;
  isCurrentMonth: boolean;
  leaveRequests: ILeaveRequest[];
}

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
  monthNames = [
    'January',
    'February',
    'March',
    'April',
    'May',
    'June',
    'July',
    'August',
    'September',
    'October',
    'November',
    'December',
  ];
  dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

  // Independent calendar filters - remove rejected
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
    return this.requests.filter((request) => {
      if (request.status === 'Pending' && this.calendarFilters.pending)
        return true;
      if (request.status === 'Approved' && this.calendarFilters.approved)
        return true;
      // Remove rejected filter completely
      return false;
    });
  }

  toggleFilter(status: 'pending' | 'approved') {
    this.calendarFilters[status] = !this.calendarFilters[status];
    this.generateCalendar();
  }

  generateCalendar() {
    const firstDay = new Date(this.currentYear, this.currentMonth, 1);
    const startDate = new Date(firstDay);
    startDate.setDate(startDate.getDate() - firstDay.getDay());

    this.calendarDays = [];
    const currentDate = new Date(startDate);

    for (let i = 0; i < 42; i++) {
      const day: CalendarDay = {
        date: new Date(currentDate),
        dayNumber: currentDate.getDate(),
        isCurrentMonth: currentDate.getMonth() === this.currentMonth,
        leaveRequests: this.getLeaveRequestsForDate(currentDate),
      };
      this.calendarDays.push(day);
      currentDate.setDate(currentDate.getDate() + 1);
    }
  }

  getLeaveRequestsForDate(date: Date): ILeaveRequest[] {
    return this.filteredRequestsForCalendar.filter((request) => {
      const fromDate = new Date(request.from);
      const toDate = new Date(request.to);
      const checkDate = new Date(date);

      return checkDate >= fromDate && checkDate <= toDate;
    });
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
    switch (status) {
      case 'Pending':
        return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 'Approved':
        return 'bg-green-100 text-green-800 border-green-200';
      case 'Rejected':
        return 'bg-red-100 text-red-800 border-red-200';
      default:
        return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  }

  isToday(date: Date): boolean {
    const today = new Date();
    return date.toDateString() === today.toDateString();
  }
}
