import { Component, EventEmitter, Output, OnInit, Input } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { RequestDetail } from '../request-detail/request-detail';
import { CalendarView } from '../calendar-view/calendar-view';
import { LeaveRequests } from '../../../../services/leave-requests/leave-requests';
import { ILeaveRequest } from '../../../../models/leave-request';
import { StatusUtils } from '../../../../utils/status.utils';
import { DateUtils } from '../../../../utils/date.utils';
import { RequestUtils } from '../../../../utils/request.utils';
import { LeaveRequestTypeService } from '../../../../services/leaveRequestType/leave-request-type-service';

@Component({
  selector: 'app-add-requests',
  standalone: true,
  imports: [CommonModule, NgClass, RequestDetail, CalendarView],
  templateUrl: './add-requests.html',
  styleUrls: ['./add-requests.css'],
})
export class AddRequests implements OnInit {
  @Input() filter: 'All' | 'Pending' | 'Approved' | 'Rejected' = 'All';
  @Input() viewMode: 'card' | 'table' | 'calendar' = 'card';
  @Input() searchTerm: string = '';

  requests: ILeaveRequest[] = [];
  selectedRequest: ILeaveRequest | null = null;

  constructor(
    private leaveRequests: LeaveRequests,
    private leaveRequestTypeService: LeaveRequestTypeService
  ) {}

  ngOnInit() {
    this.loadRequests();
  }

  public loadRequests() {
    this.leaveRequests.fetchByManager().subscribe((apiData) => {
      if (apiData.success && Array.isArray(apiData.data)) {
        const requestsRaw = apiData.data;
        this.requests = [];
        requestsRaw.forEach((item: any) => {
          this.leaveRequestTypeService
            .getLeaveRequestTypeById(item.leaveRequestTypeId)
            .subscribe((typeRes) => {
              const leaveTypeData = typeRes?.data;
              const leaveTypeDescription = leaveTypeData?.description || '';
              this.requests.push({
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
                leaveTypeDescription: leaveTypeDescription,
                leaveType: leaveTypeData
                  ? {
                      id: leaveTypeData.id,
                      title: leaveTypeData.title,
                      description: leaveTypeData.description,
                      maxDays: leaveTypeData.maxDays,
                      isPaid: leaveTypeData.isPaid,
                    }
                  : undefined,
              });

              this.requests.sort(
                (a, b) => b.createdAtDate.getTime() - a.createdAtDate.getTime()
              );
            });
        });
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
