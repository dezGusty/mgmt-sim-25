import {
  Component,
  Input,
  OnInit,
  OnChanges,
  SimpleChanges,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BehaviorSubject, Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { ILeaveRequest } from '../../../../models/leave-request';
import { CalendarUtils, CalendarDay } from '../../../../utils/calendar.utils';
import { RequestUtils } from '../../../../utils/request.utils';
import { ColorUtils } from '../../../../utils/color.utils';
import { LeaveRequests } from '../../../../services/leave-requests/leave-requests';
import { RequestDetail } from '../request-detail/request-detail';

@Component({
  selector: 'app-calendar-view',
  standalone: true,
  imports: [CommonModule, FormsModule, RequestDetail],
  templateUrl: './calendar-view.html',
  styleUrls: ['./calendar-view.css'],
})
export class CalendarView implements OnInit, OnChanges {
  filteredEmployees: string[] = [];
  employeeSearchTerm: string = '';
  private employeeSearchSubject = new BehaviorSubject<string>('');
  private destroy$ = new Subject<void>();
  @Input() requests: ILeaveRequest[] = [];

  currentDate = new Date();
  currentMonth = this.currentDate.getMonth();
  currentYear = this.currentDate.getFullYear();
  calendarDays: CalendarDay[] = [];
  monthNames = CalendarUtils.MONTH_NAMES;
  dayNames = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

  calendarFilters = {
    pending: true,
    approved: true,
    rejected: false,
  };

  hoveredRequest: ILeaveRequest | null = null;
  selectedRequest: ILeaveRequest | null = null;

  employees: string[] = [];
  calendarDates: Date[] = [];
  monthHeaders: { month: string; year: number; daysInMonth: number }[] = [];
  tableData: {
    employee: string;
    dates: { date: Date; hasLeave: boolean; requests: ILeaveRequest[] }[];
  }[] = [];

  displayMonths = 3;
  monthsOptions = [1, 3, 6];
  startMonth = this.currentMonth;
  startYear = this.currentYear;

  legendItems: { title: string; color: string }[] = [];
  isFullscreen = false;

  constructor(private leaveRequests: LeaveRequests) {}

  ngOnInit() {
    this.employeeSearchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe((searchTerm) => {
      if (searchTerm && searchTerm.trim().length > 0) {
        this.fetchRequestsByManager(searchTerm);
      } else {
        this.fetchRequestsByManager('');
      }
    });
    this.fetchRequestsByManager('');
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['requests']) {
      this.generateTableData();
    }
  }

  updateLegend() {
    const leaveTypes = new Set<string>();
    this.filteredRequestsForCalendar.forEach((req) => {
      if (req.leaveType?.title) {
        leaveTypes.add(req.leaveType.title);
      }
    });

    console.log('Unique leave types found:', Array.from(leaveTypes));

    this.legendItems = Array.from(leaveTypes).map((type) => ({
      title: type,
      color: ColorUtils.generateColorForLeaveType(type),
    }));

    console.log('Legend items created:', this.legendItems);
  }

  getLeaveTypeColor(request: ILeaveRequest): string {
    return ColorUtils.generateColorForLeaveType(
      request.leaveType?.title || 'Unknown'
    );
  }

  get filteredRequestsForCalendar(): ILeaveRequest[] {
    return RequestUtils.filterRequestsForCalendar(
      this.requests,
      this.calendarFilters
    );
  }

  toggleFilter(status: 'pending' | 'approved') {
    this.calendarFilters[status] = !this.calendarFilters[status];
    this.generateTableData();
  }

  generateTableData() {
    const filteredRequests = this.filteredRequestsForCalendar;
    this.employees = [
      ...new Set(filteredRequests.map((req) => req.employeeName)),
    ].sort();
    this.filteredEmployees = [...this.employees];
    this.generateCalendarDates();
    this.generateMonthHeaders();
    this.updateLegend();
    this.tableData = this.filteredEmployees.map((employee: string) => ({
      employee,
      dates: this.calendarDates.map((date) => ({
        date,
        hasLeave: this.hasLeaveOnDate(employee, date, filteredRequests),
        requests: this.getRequestsForEmployeeAndDate(
          employee,
          date,
          filteredRequests
        ),
      })),
    }));
  }

  onEmployeeSearchInput(event: any) {
    this.employeeSearchTerm = event.target.value;
    this.employeeSearchSubject.next(this.employeeSearchTerm);
  }

  fetchRequestsByManager(name: string) {
    this.leaveRequests.fetchByManager(name).subscribe((res) => {
      console.log('API response:', res);
      let rawData: any[] = [];
      if (res && Array.isArray(res.data)) {
        rawData = res.data;
      } else if (res && res.data && typeof res.data === 'object' && Array.isArray((res.data as any).items)) {
        rawData = (res.data as any).items;
      }
      this.requests = rawData.map((item: any) => ({
        id: item.id?.toString() ?? '',
        employeeName: item.fullName ?? item.employeeName ?? '',
        status: this.mapStatus(item.requestStatus),
        from: item.startDate ?? item.from ?? '',
        to: item.endDate ?? item.to ?? '',
        reason: item.reason ?? '',
        createdAt: item.createdAt ?? '',
        createdAtDate: new Date(item.createdAt ?? Date.now()),
        comment: item.reviewerComment ?? item.comment ?? '',
        departmentName: item.departmentName ?? '',
        leaveType: item.leaveRequestTypeName || item.leaveTypeName || item.leaveTypeTitle
          ? {
              title: item.leaveRequestTypeName || item.leaveTypeName || item.leaveTypeTitle,
              isPaid: item.isPaid ?? false,
            }
          : undefined,
      }));
      this.generateTableData();
    });
  }

  private mapStatus(requestStatus: number | string | undefined): any {
    if (typeof requestStatus === 'string') return requestStatus;
    switch (requestStatus) {
      case 1:
        return 'Pending';
      case 4:
        return 'Approved';
      case 8:
        return 'Rejected';
      default:
        return 'Pending';
    }
  }

  generateCalendarDates() {
    this.calendarDates = [];

    for (let i = 0; i < this.displayMonths; i++) {
      const month = (this.startMonth + i) % 12;
      const year = this.startYear + Math.floor((this.startMonth + i) / 12);
      const daysInMonth = new Date(year, month + 1, 0).getDate();

      for (let day = 1; day <= daysInMonth; day++) {
        this.calendarDates.push(new Date(year, month, day));
      }
    }
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  getRequestsForEmployeeAndDate(
    employee: string,
    date: Date,
    requests: ILeaveRequest[]
  ): ILeaveRequest[] {
    return requests.filter(
      (req) =>
        req.employeeName === employee &&
        this.isDateInRange(date, new Date(req.from), new Date(req.to))
    );
  }

  onMonthsChange(months: number) {
    this.displayMonths = months;
    this.generateTableData();
  }

  onWheel(event: WheelEvent) {
    const container = event.currentTarget as HTMLElement;
    event.preventDefault();

    if (event.shiftKey) {
      container.scrollLeft += event.deltaY;
    } else {
      container.scrollTop += event.deltaY;
    }
  }

  isDateInRange(date: Date, fromDate: Date, toDate: Date): boolean {
    const checkDate = new Date(
      date.getFullYear(),
      date.getMonth(),
      date.getDate()
    );
    const from = new Date(
      fromDate.getFullYear(),
      fromDate.getMonth(),
      fromDate.getDate()
    );
    const to = new Date(
      toDate.getFullYear(),
      toDate.getMonth(),
      toDate.getDate()
    );

    return checkDate >= from && checkDate <= to;
  }

  onCellClick(employee: string, date: Date) {
    const requests = this.getRequestsForEmployeeAndDate(
      employee,
      date,
      this.filteredRequestsForCalendar
    );
    if (requests.length > 0) {
      this.selectedRequest = requests[0];
    }
  }

  previousMonth() {
    if (this.startMonth === 0) {
      this.startMonth = 11;
      this.startYear--;
    } else {
      this.startMonth--;
    }
    this.generateTableData();
  }

  nextMonth() {
    if (this.startMonth === 11) {
      this.startMonth = 0;
      this.startYear++;
    } else {
      this.startMonth++;
    }
    this.generateTableData();
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

  isDateInHoveredRequest(date: Date, employee: string): boolean {
    if (!this.hoveredRequest) {
      return false;
    }

    if (this.hoveredRequest.employeeName !== employee) {
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
        if (res.success) {
          this.closeDetails();
          const requestIndex = this.requests.findIndex(
            (req) => req.id === data.id
          );
          if (requestIndex !== -1) {
            this.requests[requestIndex].status = 'Approved';
            this.requests[requestIndex].comment = data.comment || '';
            this.requests = [...this.requests];
          }
          this.generateTableData();
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
        if (res.success) {
          this.closeDetails();
          const requestIndex = this.requests.findIndex(
            (req) => req.id === data.id
          );
          if (requestIndex !== -1) {
            this.requests[requestIndex].status = 'Rejected';
            this.requests[requestIndex].comment = data.comment;
            this.requests = [...this.requests];
          }
          this.generateTableData();
        }
      });
  }

  getStatusIcon(status: string | undefined): string {
    if (!status) return '•';

    switch (status.toLowerCase()) {
      case 'approved':
        return '✓';
      case 'pending':
        return '?';
      default:
        return '•';
    }
  }

  onRequestAdded(newRequest: any) {
    this.requests = [...this.requests, newRequest];
    this.generateTableData();
  }

  refreshCalendarData() {
    this.generateTableData();
  }

  toggleFullscreen() {
    this.isFullscreen = !this.isFullscreen;
  }

  generateMonthHeaders() {
    this.monthHeaders = [];
    for (let i = 0; i < this.displayMonths; i++) {
      const month = (this.startMonth + i) % 12;
      const year = this.startYear + Math.floor((this.startMonth + i) / 12);
      const daysInMonth = new Date(year, month + 1, 0).getDate();
      this.monthHeaders.push({
        month: this.monthNames[month],
        year,
        daysInMonth,
      });
    }
  }

  hasLeaveOnDate(employee: string, date: Date, requests: ILeaveRequest[]): boolean {
    return requests.some(
      (req) =>
        req.employeeName === employee &&
        this.isDateInRange(date, new Date(req.from), new Date(req.to))
    );
  }
}
