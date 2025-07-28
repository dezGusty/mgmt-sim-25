import { Component, EventEmitter, Output, OnInit, Input } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { RequestDetail } from '../request-detail/request-detail';
import { CalendarView } from '../calendar-view/calendar-view';
import { LeaveRequests } from '../../../../services/leave-requests/leave-requests';
import { ILeaveRequest } from '../../../../models/leave-request';
import { IRequestStats } from '../../../../models/request-stats';
import { StatusUtils } from '../../../../utils/status.utils';
import { DateUtils } from '../../../../utils/date.utils';

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

  requests: ILeaveRequest[] = [];
  selectedRequest: ILeaveRequest | null = null;

  constructor(private leaveRequests: LeaveRequests) {}

  ngOnInit() {
    this.loadRequests();
  }

  public loadRequests() {
    this.leaveRequests.fetchByManager().subscribe((apiData) => {
      if (Array.isArray(apiData)) {
        console.log(apiData);
        this.requests = apiData
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

        const stats = this.calculateStats(this.requests);
        this.statsUpdated.emit(stats);
      }
    });
  }

  calculateStats(requests: ILeaveRequest[]): IRequestStats {
    const total = requests.length;
    const pending = requests.filter((r) => r.status === 'Pending').length;
    const approved = requests.filter((r) => r.status === 'Approved').length;
    const rejected = requests.filter((r) => r.status === 'Rejected').length;
    return { total, pending, approved, rejected };
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
          const req = this.requests.find((r) => r.id === data.id);
          if (req) {
            req.status = 'Approved';
            req.comment = data.comment;
          }
          this.closeDetails();
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
          const req = this.requests.find((r) => r.id === data.id);
          if (req) {
            req.status = 'Rejected';
            req.comment = data.comment;
          }
          this.closeDetails();
        }
      });
  }

  public addNewRequestToList(createdRequest: any) {
    const newRequest: ILeaveRequest = {
      id: String(createdRequest.id),
      employeeName: createdRequest.fullName || 'Unknown Employee',
      status: StatusUtils.mapStatus(createdRequest.requestStatus || 2),
      from: DateUtils.formatDate(createdRequest.startDate),
      to: DateUtils.formatDate(createdRequest.endDate),
      days: DateUtils.calcDays(
        createdRequest.startDate,
        createdRequest.endDate
      ),
      reason: createdRequest.reason,
      createdAt: DateUtils.formatDate(
        createdRequest.createdAt || new Date().toISOString()
      ),
      comment: createdRequest.reviewerComment || '',
      createdAtDate: new Date(
        createdRequest.createdAt || new Date().toISOString()
      ),
    };

    this.requests.unshift(newRequest);

    const stats = this.calculateStats(this.requests);
    this.statsUpdated.emit(stats);
  }

  get filteredRequests(): ILeaveRequest[] {
    if (this.filter === 'All') {
      return this.requests;
    }
    return this.requests.filter((request) => request.status === this.filter);
  }
}
