import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LeaveRequestService } from '../../../services/leaveRequest/leaveRequest.service';
import { LeaveRequest } from '../../../models/entities/iLeave-request';
import { ILeaveRequestType } from '../../../models/entities/ileave-request-type';
import { LeaveRequestTypeService } from '../../../services/leaveRequestType/leave-request-type-service';
import { RequestStatus } from '../../../models/enums/RequestStatus';

@Component({
  selector: 'app-user-requests-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './user-requests-list.html',
  styleUrl: './user-requests-list.css',
})
export class UserRequestsList implements OnInit {
  @Output() close = new EventEmitter<void>();

  RequestStatus = RequestStatus;
  
  requests: LeaveRequest[] = [];
  filteredRequests: LeaveRequest[] = [];
  selectedRequest: LeaveRequest | null = null;
  isLoading = true;
  statusFilter: 'all' | 'pending' | 'approved' | 'rejected' = 'all';
  searchTerm = '';
  showListModal = true;
  errorMessage = '';

  leaveRequestTypes: ILeaveRequestType[] = [];
  isLoadingTypes = true;

  constructor(
    private leaveRequestService: LeaveRequestService,
    private leaveRequestTypeService: LeaveRequestTypeService
  ) 
  {}

  ngOnInit() {
    this.loadLeaveRequestTypes();
    this.loadRequests();
  }

  loadLeaveRequestTypes() {
    this.isLoadingTypes = true;
    
    this.leaveRequestTypeService.getAllLeaveRequestTypes().subscribe({
      next: (types) => {
        this.leaveRequestTypes = types.data;
        this.isLoadingTypes = false;
      },
      error: (err) => {
        this.isLoadingTypes = false;
        this.errorMessage = 'Failed to load leave request types.';
      }
    });
  }

   getLeaveRequestTypeName(typeId: number): string {
    if (this.isLoadingTypes) {
      return 'Loading...';
    }
    
    const type = this.leaveRequestTypes.find(t => t.id === typeId);
    return type ? type.description || type.description : 'Unknown';
  }

  loadRequests() {
    this.isLoading = true;
    this.errorMessage = '';

    this.leaveRequestService.getCurrentUserLeaveRequests().subscribe({
      next: (data) => {       
        this.requests = data.data;
        this.filteredRequests = [...this.requests];
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading requests:', err);
        console.error('Error details:', {
          message: err.message,
          status: err.status,
          statusText: err.statusText,
          error: err.error
        });
        this.isLoading = false;

        if (err.status === 401) {
          this.errorMessage = 'You are not authorized. Please log in again.';
        } else if (err.status === 404) {
          this.errorMessage = 'No leave requests found.';
        } else if (err.status === 400) {
          this.errorMessage = err.error?.message || 'Invalid request.';
        } else {
          this.errorMessage = err.error?.message || 'Failed to load requests.';
        }
      }
    })
  }

  refreshRequests() {
    this.loadRequests();
  }

  setStatusFilter(status: 'all' | 'pending' | 'approved' | 'rejected') {
    this.statusFilter = status;
    this.filterRequests();
  }


  filterRequests() {
    let results = this.requests;
    
    // Apply status filter
     if (this.statusFilter !== 'all') {
      results = results.filter(req => {
        switch (this.statusFilter) {
          case 'pending': return req.requestStatus === RequestStatus.PENDING;
          case 'approved': return req.requestStatus === RequestStatus.APPROVED;
          case 'rejected': return req.requestStatus === RequestStatus.REJECTED;
          default: return true;
        }
      });
    }
    
    // Apply search term
    if (this.searchTerm.trim()) {
      const term = this.searchTerm.toLowerCase().trim();
      results = results.filter(req => {
          const typeName = this.getLeaveRequestTypeName(req.leaveRequestTypeId).toLowerCase();
          return (
            typeName.includes(term) ||
            req.reason?.toLowerCase().includes(term) ||
            req.id?.toString().includes(term)
          );
      });
    }
    
    this.filteredRequests = results;
  }

  getStatusDisplayName(status: RequestStatus): string {
      switch (status) {
        case RequestStatus.PENDING:
          return 'Pending';
        case RequestStatus.APPROVED:
          return 'Approved';
        case RequestStatus.REJECTED:
          return 'Rejected';
        default:
          return 'Unknown';
      }
    }

  viewDetails(request: LeaveRequest) {
    this.selectedRequest = {...request};
    this.showListModal = false;
  }

  closeDetails() {
    this.selectedRequest = null;
    this.showListModal = true;
  }

  cancelRequest(request: LeaveRequest) {
    if (confirm('Are you sure you want to cancel this request?')) {
      if(!request.id) {
        return;
      }

      this.leaveRequestService.cancelLeaveRequest(request.id).subscribe({
        next: () => {
          const index = this.requests.findIndex(r => r.id === request.id);
          if (index !== -1) {
            this.requests.splice(index, 1);
            this.filterRequests();
            
            if (this.selectedRequest && this.selectedRequest.id === request.id) {
              this.selectedRequest = null;
              this.showListModal = true;
            }
          }
        },
        error: (err) => {
          this.errorMessage = err.error?.message || 'Failed to cancel request.';
        }
      });
    }
  }
}