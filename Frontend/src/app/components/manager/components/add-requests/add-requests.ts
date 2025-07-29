import { Component, EventEmitter, Output, OnInit, Input } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { RequestDetail } from '../request-detail/request-detail';
import { CalendarView } from '../calendar-view/calendar-view';
import { LeaveRequests } from '../../../../services/leave-requests/leave-requests';
import { ILeaveRequest } from '../../../../models/leave-request';
import { IRequestStats } from '../../../../models/request-stats';
import { StatusUtils } from '../../../../utils/status.utils';
import { DateUtils } from '../../../../utils/date.utils';
import { RequestUtils } from '../../../../utils/request.utils';

@Component({
  selector: 'app-add-requests',
  standalone: true,
  imports: [CommonModule, NgClass, RequestDetail, CalendarView],
  templateUrl: './add-requests.html',
  styleUrls: ['./add-requests.css'],
})
export class AddRequests implements OnInit {
  @Output() statsUpdated = new EventEmitter<IRequestStats>();
  @Input() filter: 'All' | 'Pending' | 'Approved' | 'Rejected' = 'All';
  @Input() viewMode: 'card' | 'table' | 'calendar' = 'card';
  @Input() searchTerm: string = '';

  requests: ILeaveRequest[] = [];
  selectedRequest: ILeaveRequest | null = null;

  constructor(private leaveRequests: LeaveRequests) {}

  ngOnInit() {
    this.loadRequests();
  }

  public loadRequests() {
    this.leaveRequests.fetchByManager().subscribe((apiData) => {
      if (apiData.success && Array.isArray(apiData.data)) {
        this.requests = apiData.data
          .map((item: any) => ({
            id: String(item.id),
            employeeName: item.fullName,
            status: StatusUtils.mapStatus(item.requestStatus),
            from: DateUtils.formatDate(item.startDate),
            to: DateUtils.formatDate(item.endDate),
            days: DateUtils.calcDays(item.startDate, item.endDate),
            reason: item.reason,
            createdAt: DateUtils.formatDate(item.createdAt),
            comment: item.reviewerComment,
            createdAtDate: new Date(item.createdAt),
          }))
          .sort(
            (a, b) => b.createdAtDate.getTime() - a.createdAtDate.getTime()
          );

        const stats = RequestUtils.calculateStats(this.requests);
        this.statsUpdated.emit(stats);
      }
    });
  }

  calculateStats(requests: ILeaveRequest[]): IRequestStats {
    return RequestUtils.calculateStats(requests);
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
          this.loadRequests();
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
          this.loadRequests();
        }
      });
  }

  get filteredRequests(): ILeaveRequest[] {
    let filtered = RequestUtils.filterRequests(this.requests, this.filter);

    if (this.searchTerm) {
      filtered = filtered.filter((request) =>
        request.employeeName
          .toLowerCase()
          .includes(this.searchTerm.toLowerCase())
      );
    }

    return filtered;
  }
}
