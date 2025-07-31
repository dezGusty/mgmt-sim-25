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
import { LeaveRequests } from '../../../../services/leave-requests/leave-requests';
import { RequestDetail } from '../request-detail/request-detail';

@Component({
  selector: 'app-calendar-view',
  standalone: true,
  imports: [CommonModule, RequestDetail],
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
  dayNames = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

  calendarFilters = {
    pending: false,
    approved: true,
    rejected: false,
  };

  hoveredRequest: ILeaveRequest | null = null;
  selectedRequest: ILeaveRequest | null = null;

  constructor(private leaveRequests: LeaveRequests) {}

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
    this.calendarDays = CalendarUtils.generateCalendarDaysWithMondayFirst(
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

  onRequestHover(request: ILeaveRequest) {
    this.hoveredRequest = request;
  }

  onRequestLeave() {
    this.hoveredRequest = null;
  }

  isDateInHoveredRequest(date: Date): boolean {
    if (!this.hoveredRequest) {
      return false;
    }

    const fromDate = new Date(this.hoveredRequest.from);
    const toDate = new Date(this.hoveredRequest.to);
    const checkDate = new Date(date);

    return checkDate >= fromDate && checkDate <= toDate;
  }

  openDetails(req: ILeaveRequest) {
    this.selectedRequest = req;
  }

  closeDetails() {
    this.selectedRequest = null;
  }

  onApprove(data: { id: string; comment?: string }) {
    this.leaveRequests
      .patchLeaveRequest({
        id: data.id,
        requestStatus: 4,
        reviewerComment: data.comment,
      })
      .subscribe((res) => {
        if (res) {
          this.closeDetails();
          this.generateCalendar();
        }
      });
  }

  onReject(data: { id: string; comment?: string }) {
    this.leaveRequests
      .patchLeaveRequest({
        id: data.id,
        requestStatus: 8,
        reviewerComment: data.comment,
      })
      .subscribe((res) => {
        if (res) {
          this.closeDetails();
          this.generateCalendar();
        }
      });
  }
}
