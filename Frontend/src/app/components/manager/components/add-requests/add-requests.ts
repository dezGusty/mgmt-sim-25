import { Component, EventEmitter, Output, OnInit } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { RequestDetail } from '../request-detail/request-detail';
import { LeaveRequests } from '../../../../services/leave-requests/leave-requests';
import { ILeaveRequest } from '../../../../models/leave-request';
import { IRequestStats } from '../../../../models/request-stats';

@Component({
  selector: 'app-add-requests',
  standalone: true,
  imports: [CommonModule, NgClass, RequestDetail],
  templateUrl: './add-requests.html',
  styleUrls: ['./add-requests.css'],
})
export class AddRequests implements OnInit {
  @Output() statsUpdated = new EventEmitter<IRequestStats>();

  requests: ILeaveRequest[] = [];
  selectedRequest: ILeaveRequest | null = null;

  constructor(private leaveRequests: LeaveRequests) {}

  ngOnInit() {
    this.loadRequests();
  }

  loadRequests() {
    this.leaveRequests.fetchByManager().subscribe((apiData) => {
      if (Array.isArray(apiData)) {
        this.requests = apiData.map((item: any) => ({
          id: String(item.id),
          employeeName: item.fullName,
          status: this.mapStatus(item.requestStatus),
          from: this.formatDate(item.startDate),
          to: this.formatDate(item.endDate),
          days: this.calcDays(item.startDate, item.endDate),
          reason: item.reason,
          createdAt: this.formatDate(item.createdAt),
          comment: item.reviewerComment,
        }));

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

  mapStatus(status: number): 'Pending' | 'Approved' | 'Rejected' {
    switch (status) {
      case 2:
        return 'Pending';
      case 4:
        return 'Approved';
      case 8:
        return 'Rejected';
      default:
        return 'Pending';
    }
  }

  formatDate(dateStr: string): string {
    const d = new Date(dateStr);
    return d.toLocaleString('en-GB', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
    });
  }

  calcDays(start: string, end: string): number {
    const d1 = new Date(start);
    const d2 = new Date(end);
    return Math.max(
      1,
      Math.ceil((d2.getTime() - d1.getTime()) / (1000 * 60 * 60 * 24)) + 1
    );
  }

  openDetails(req: ILeaveRequest) {
    this.selectedRequest = req;
  }

  closeDetails() {
    this.selectedRequest = null;
  }

  onApprove(data: { id: string; comment?: string }) {
    const req = this.requests.find((r) => r.id === data.id);
    if (req) {
      req.status = 'Approved';
      req.comment = data.comment;
    }
    this.closeDetails();
  }

  onReject(data: { id: string; comment?: string }) {
    const req = this.requests.find((r) => r.id === data.id);
    if (req) {
      req.status = 'Rejected';
      req.comment = data.comment;
    }
    this.closeDetails();
  }
}
