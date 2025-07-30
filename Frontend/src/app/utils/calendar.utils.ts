import { ILeaveRequest } from '../models/leave-request';

export interface CalendarDay {
  date: Date;
  dayNumber: number;
  isCurrentMonth: boolean;
  leaveRequests: ILeaveRequest[];
}

export class CalendarUtils {
  static readonly MONTH_NAMES = [
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

  static readonly DAY_NAMES = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

  static generateCalendarDaysWithMondayFirst(
    month: number,
    year: number,
    requests: ILeaveRequest[]
  ): CalendarDay[] {
    const firstDay = new Date(year, month, 1);
    const startDate = new Date(firstDay);

    const dayOfWeek = (firstDay.getDay() + 6) % 7;
    startDate.setDate(startDate.getDate() - dayOfWeek);

    const calendarDays: CalendarDay[] = [];
    const currentDate = new Date(startDate);

    for (let i = 0; i < 42; i++) {
      const day: CalendarDay = {
        date: new Date(currentDate),
        dayNumber: currentDate.getDate(),
        isCurrentMonth: currentDate.getMonth() === month,
        leaveRequests: this.getLeaveRequestsForDate(currentDate, requests),
      };
      calendarDays.push(day);
      currentDate.setDate(currentDate.getDate() + 1);
    }

    return calendarDays;
  }

  static getLeaveRequestsForDate(
    date: Date,
    requests: ILeaveRequest[]
  ): ILeaveRequest[] {
    return requests.filter((request) => {
      const fromDate = new Date(request.from);
      const toDate = new Date(request.to);
      const checkDate = new Date(date);

      return checkDate >= fromDate && checkDate <= toDate;
    });
  }

  static isToday(date: Date): boolean {
    const today = new Date();
    return date.toDateString() === today.toDateString();
  }

  static getStatusColor(status: string): string {
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
}
