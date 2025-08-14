import { Component, EventEmitter, Output, OnInit, Input, OnChanges, SimpleChanges } from '@angular/core';
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
export class AddRequests implements OnInit, OnChanges {
  @Input() filter: 'All' | 'Pending' | 'Approved' | 'Rejected' = 'All';
  @Input() viewMode: 'card' | 'table' | 'calendar' = 'table';
  @Input() searchTerm: string = '';
  @Input() searchCriteria: 'all' | 'employee' | 'department' | 'type' = 'all';
  @Output() dataRefreshed = new EventEmitter<void>();

  requests: ILeaveRequest[] = [];
  selectedRequest: ILeaveRequest | null = null;
  errorMessage: string | null = null;

  sortColumn: string = 'submitted';
  sortDirection: 'asc' | 'desc' = 'desc';

  currentPage: number = 1;
  itemsPerPage: number = 10;
  totalPages: number = 0;
  totalCount: number = 0;
  isLoading: boolean = false;

  constructor(private leaveRequests: LeaveRequests) {}

  Math = Math;

  ngOnInit() {
    this.loadRequests();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['filter'] || changes['searchTerm'] || changes['searchCriteria']) {
      this.currentPage = 1;
      this.totalPages = 0;
      this.totalCount = 0;
      this.loadRequests();
    }
  }
  addRequest(newRequest: ILeaveRequest) {
    this.requests = [newRequest, ...this.requests];
  }

  public loadRequests() {
    if (this.isLoading) return;
    
    this.isLoading = true;
    this.errorMessage = null;
    
    if (this.searchTerm && this.searchTerm.trim()) {
      this.loadRequestsWithSearch();
    } else {
      this.loadRequestsPaginated();
    }
  }

  private loadRequestsPaginated() {
    const statusFilter = this.filter === 'All' ? 'ALL' : this.filter;
    
    this.leaveRequests.fetchByManagerPaginated(statusFilter, this.itemsPerPage, this.currentPage).subscribe({
      next: (apiData) => {
        if (apiData.success && apiData.data) {
          const requestsRaw = apiData.data.items || [];
          this.totalCount = apiData.data.totalCount;
          this.totalPages = apiData.data.totalPages;

          this.requests = this.mapRequests(requestsRaw);
          this.errorMessage = null;
        } else {
          this.requests = [];
          this.totalCount = 0;
          this.totalPages = 0;
          this.errorMessage = apiData.message || 'Failed to load leave requests.';
        }
        this.isLoading = false;
      },
      error: () => {
        this.requests = [];
        this.totalCount = 0;
        this.totalPages = 0;
        this.errorMessage = 'Failed to load leave requests. Please try again later.';
        this.isLoading = false;
      },
    });
  }

  private loadRequestsWithSearch() {
    this.leaveRequests.fetchByManager().subscribe({
      next: (apiData) => {
        if (apiData.success && Array.isArray(apiData.data)) {
          const requestsRaw = apiData.data;
          let filteredRequests = this.mapRequests(requestsRaw);
          
          filteredRequests = RequestUtils.filterRequests(filteredRequests, this.filter);
          
          if (this.searchTerm && this.searchTerm.trim()) {
            filteredRequests = filteredRequests.filter((request) => {
              const searchTermLower = this.searchTerm.toLowerCase();

              switch (this.searchCriteria) {
                case 'employee':
                  return request.employeeName.toLowerCase().includes(searchTermLower);
                case 'department':
                  return (request.departmentName || '')
                    .toLowerCase()
                    .includes(searchTermLower);
                case 'type':
                  return (request.leaveType?.title || '')
                    .toLowerCase()
                    .includes(searchTermLower);
                case 'all':
                default:
                  return (
                    request.employeeName.toLowerCase().includes(searchTermLower) ||
                    (request.departmentName || '')
                      .toLowerCase()
                      .includes(searchTermLower) ||
                    (request.leaveType?.title || '')
                      .toLowerCase()
                      .includes(searchTermLower)
                  );
              }
            });
          }

          this.totalCount = filteredRequests.length;
          this.totalPages = Math.ceil(this.totalCount / this.itemsPerPage);
          
          const startIndex = (this.currentPage - 1) * this.itemsPerPage;
          const endIndex = startIndex + this.itemsPerPage;
          this.requests = filteredRequests
            .sort((a, b) => b.createdAtDate.getTime() - a.createdAtDate.getTime())
            .slice(startIndex, endIndex);

          this.errorMessage = null;
        } else {
          this.requests = [];
          this.totalCount = 0;
          this.totalPages = 0;
          this.errorMessage = apiData.message || 'Failed to load leave requests.';
        }
        this.isLoading = false;
      },
      error: () => {
        this.requests = [];
        this.totalCount = 0;
        this.totalPages = 0;
        this.errorMessage = 'Failed to load leave requests. Please try again later.';
        this.isLoading = false;
      },
    });
  }

  private mapRequests(requestsRaw: any[]): ILeaveRequest[] {
    return requestsRaw
      .map((item: any) => {
        const status = StatusUtils.mapStatus(item.requestStatus);
        return {
          id: String(item.id),
          employeeName: item.fullName,
          status: status,
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
        };
      })
      .filter((request) => request.status !== undefined);
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
    return this.sortRequests(this.requests);
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage && !this.isLoading) {
      this.currentPage = page;
      this.loadRequests();
    }
  }

  goToFirstPage(): void {
    this.goToPage(1);
  }

  goToLastPage(): void {
    this.goToPage(this.totalPages);
  }

  goToPreviousPage(): void {
    if (this.currentPage > 1) {
      this.goToPage(this.currentPage - 1);
    }
  }

  goToNextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.goToPage(this.currentPage + 1);
    }
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisiblePages = 5;
    
    if (this.totalPages <= maxVisiblePages) {
      for (let i = 1; i <= this.totalPages; i++) {
        pages.push(i);
      }
    } else {
      let startPage = Math.max(1, this.currentPage - 2);
      let endPage = Math.min(this.totalPages, this.currentPage + 2);
      
      if (this.currentPage <= 3) {
        endPage = Math.min(this.totalPages, 5);
      }
      if (this.currentPage >= this.totalPages - 2) {
        startPage = Math.max(1, this.totalPages - 4);
      }
      
      for (let i = startPage; i <= endPage; i++) {
        pages.push(i);
      }
    }
    
    return pages;
  }

  onFilterChange(): void {
    if (!this.isLoading) {
      this.currentPage = 1;
      this.totalPages = 0;
      this.totalCount = 0;
      this.loadRequests();
    }
  }
}
