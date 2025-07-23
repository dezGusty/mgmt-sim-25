import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LeaveRequestService } from '../../../services/leaveRequest.service';
import { LeaveRequest } from '../../../models/entities/LeaveRequest';
import { LeaveRequestType } from '../../../models/entities/LeaveRequestType';
import { LeaveRequestTypeService } from '../../../services/leaveRequestType/leave-request-type';
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

  leaveRequestTypes: LeaveRequestType[] = [];
  isLoadingTypes = true;

  constructor(
    private leaveRequestService: LeaveRequestService,
    private leaveRequestTypeService: LeaveRequestTypeService
  ) {
    console.log('📋 UserRequestsList component initialized');
  }

  ngOnInit() {
    console.log('🎯 UserRequestsList ngOnInit called');
    this.loadLeaveRequestTypes();
    this.loadRequests();
  }

  loadLeaveRequestTypes() {
    console.log('📥 Loading leave request types from database...');
    this.isLoadingTypes = true;
    
    this.leaveRequestTypeService.getAllLeaveRequestTypes().subscribe({
      next: (types) => {
        console.log('✅ Leave request types loaded:', types);
        this.leaveRequestTypes = types;
        this.isLoadingTypes = false;
      },
      error: (err) => {
        console.error('❌ Error loading leave request types:', err);
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
    console.log('📥 Loading user requests...');
    this.isLoading = true;
    const userId = 1; 
    console.log('👤 Loading requests for userId:', userId);

    this.leaveRequestService.getUserLeaveRequests(userId).subscribe({
      next: (data) => {
        console.log('✅ Requests loaded successfully!', data);
        console.log('📊 Number of requests:', data.length);
        this.requests = data;
        this.filteredRequests = [...this.requests];
        this.isLoading = false;
        console.log('📋 Filtered requests:', this.filteredRequests);
      },
      error: (err) => {
        console.error('❌ Error loading requests:', err);
        console.error('❌ Error details:', {
          message: err.message,
          status: err.status,
          statusText: err.statusText,
          error: err.error
        });
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Failed to load requests.';
      }
    })
  }

  setStatusFilter(status: 'all' | 'pending' | 'approved' | 'rejected') {
    console.log('🔍 Setting status filter to:', status);
    this.statusFilter = status;
    this.filterRequests();
  }

  

  filterRequests() {
    console.log('🔍 Filtering requests with status:', this.statusFilter, 'and search term:', this.searchTerm);
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
    console.log('📊 Filtered results count:', this.filteredRequests.length);
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
    console.log('👁️ Viewing details for request:', request);
    this.selectedRequest = {...request};
    this.showListModal = false;
  }

  closeDetails() {
    console.log('❌ Closing request details');
    this.selectedRequest = null;
    this.showListModal = true;
  }

  cancelRequest(request: LeaveRequest) {
    console.log('🗑️ Attempting to cancel request:', request);
    if (confirm('Are you sure you want to cancel this request?')) {
      if(!request.id) {
        console.error('❌ Cannot cancel request - no ID found');
        return;
      }

      console.log('🔗 Calling leaveRequestService.cancelLeaveRequest() for ID:', request.id);
      this.leaveRequestService.cancelLeaveRequest(request.id).subscribe({
        next: () => {
          console.log('✅ Request cancelled successfully');
          const index = this.requests.findIndex(r => r.id === request.id);
          if (index !== -1) {
            console.log('🔄 Removing request from local array at index:', index);
            this.requests.splice(index, 1);
            this.filterRequests();
            
            if (this.selectedRequest && this.selectedRequest.id === request.id) {
              this.selectedRequest = null;
              this.showListModal = true;
            }
          }
        },
        error: (err) => {
          console.error('❌ Error cancelling request:', err);
          this.errorMessage = err.error?.message || 'Failed to cancel request.';
        }
      });
    }
  }
}