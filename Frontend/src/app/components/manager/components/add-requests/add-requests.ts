import { Component, EventEmitter, Output, OnInit, Input } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { RequestDetail } from '../request-detail/request-detail';
import { CalendarView } from '../calendar-view/calendar-view';
import { LeaveRequests } from '../../../../services/leave-requests/leave-requests';
import { ILeaveRequest } from '../../../../models/leave-request';
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
  @Input() filter: 'All' | 'Pending' | 'Approved' | 'Rejected' = 'All';
  @Input() viewMode: 'card' | 'table' | 'calendar' = 'table';
  @Input() searchTerm: string = '';
  @Output() dataRefreshed = new EventEmitter<void>();

  requests: ILeaveRequest[] = [];
  selectedRequest: ILeaveRequest | null = null;

  sortColumn: string = 'submitted';
  sortDirection: 'asc' | 'desc' = 'desc';

  constructor(private leaveRequests: LeaveRequests) {}

  ngOnInit() {
    this.loadRequests();
  }
  addRequest(newRequest: ILeaveRequest) {
    this.requests = [newRequest, ...this.requests];
  }

  public loadRequests() {
    this.leaveRequests.fetchByManager().subscribe((apiData) => {
      if (apiData.success && Array.isArray(apiData.data)) {
        const requestsRaw = apiData.data;

        this.requests = requestsRaw
          .filter((item: any) => item.requestStatus !== 32)
          .map((item: any) => ({
            id: String(item.id),
            employeeName: item.fullName,
            status: StatusUtils.mapStatus(item.requestStatus),
            from: DateUtils.formatDate(item.startDate),
            to: DateUtils.formatDate(item.endDate),
            reason: item.reason,
            createdAt: DateUtils.formatDate(item.createdAt),
            comment: item.reviewerComment,
            createdAtDate: new Date(item.createdAt),
            departmentName: item.departmentName,
            leaveType: {
              id: item.leaveRequestTypeId,
              title: item.leaveRequestTypeName || 'Unknown',
              description: '',
              maxDays: 0,
              isPaid: false,
            },
          }))
          .sort(
            (a, b) => b.createdAtDate.getTime() - a.createdAtDate.getTime()
          );
      }
    });
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
            this.requests[requestIndex] = {
              ...this.requests[requestIndex],
              status: 'Approved',
              comment: data.comment || '',
            };
          }
          this.dataRefreshed.emit();
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
            this.requests[requestIndex] = {
              ...this.requests[requestIndex],
              status: 'Rejected',
              comment: data.comment || '',
            };
          }
          this.dataRefreshed.emit();
        }
      });
  }

  onActionCompleted() {
    this.dataRefreshed.emit();
  }

  sortBy(column: string) {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
  }

  private sortRequests(requests: ILeaveRequest[]): ILeaveRequest[] {
    return [...requests].sort((a, b) => {
      let aValue: any;
      let bValue: any;

      switch (this.sortColumn) {
        case 'employee':
          aValue = a.employeeName.toLowerCase();
          bValue = b.employeeName.toLowerCase();
          break;
        case 'department':
          aValue = (a.departmentName || '').toLowerCase();
          bValue = (b.departmentName || '').toLowerCase();
          break;
        case 'type':
          aValue = (a.leaveType?.title || '').toLowerCase();
          bValue = (b.leaveType?.title || '').toLowerCase();
          break;
        case 'dates':
          aValue = new Date(a.from);
          bValue = new Date(b.from);
          break;
        case 'submitted':
        default:
          aValue = a.createdAtDate;
          bValue = b.createdAtDate;
          break;
      }

      if (aValue < bValue) {
        return this.sortDirection === 'asc' ? -1 : 1;
      }
      if (aValue > bValue) {
        return this.sortDirection === 'asc' ? 1 : -1;
      }
      return 0;
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

    return this.sortRequests(filtered);
  }
}
