import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserRequestForm } from './user-request-form/user-request-form';
import { UserLeaveBalance } from './user-leave-balance/user-leave-balance';
import { CustomNavbar } from '../shared/custom-navbar/custom-navbar';
import { LeaveRequestService } from '../../services/leaveRequest/leaveRequest.service';
import { LeaveRequest } from '../../models/entities/iLeave-request';
import { ILeaveRequestType } from '../../models/entities/ileave-request-type';
import { LeaveRequestTypeService } from '../../services/leaveRequestType/leave-request-type-service';
import { RequestStatus } from '../../models/enums/RequestStatus';


@Component({
  selector: 'app-user',
  templateUrl: './user.html',
  styleUrl: './user.css',
  imports: [CommonModule,FormsModule,  UserRequestForm, UserLeaveBalance, CustomNavbar],
})
export class User {
  showRequestForm = false;
  showLeaveBalance = false;

  showSuccessMessage = false;
  successMessage = '';

  RequestStatus = RequestStatus;
  Math = Math;
  
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

  // Pagination properties
  currentPage = 1;
  itemsPerPage = 5;
  totalPages = 0;
  paginatedRequests: LeaveRequest[] = [];

  constructor(
    private router: Router,
    private leaveRequestService: LeaveRequestService,
    private leaveRequestTypeService: LeaveRequestTypeService
  ) {}

  ngOnInit() {
    this.loadLeaveRequestTypes();
    this.loadRequests();
  }

  goBack() {
    this.router.navigate(['/']);
  }
  
  toggleRequestForm() {
    this.showRequestForm = !this.showRequestForm;
  }

  

  toggleLeaveBalance() {
    this.showLeaveBalance = !this.showLeaveBalance;
  }

  onRequestSubmitted() {
    this.showSuccessMessage = true;
    this.successMessage = 'Leave request submitted successfully! 🎉';
    
    this.showRequestForm = false;
    
    this.loadRequests();
    
    setTimeout(() => {
      this.showSuccessMessage = false;
    }, 5000);
  }

  closeSuccessMessage() {
    this.showSuccessMessage = false;
  }

  // My Requests List methods
  loadRequests() {
    this.isLoading = true;
    this.errorMessage = '';

    this.leaveRequestService.getCurrentUserLeaveRequests().subscribe({
      next: (data) => {       
        this.requests = data.data.sort((a, b) => {
          const dateA = new Date( a.startDate);
          const dateB = new Date(b.startDate);
          return dateB.getTime() - dateA.getTime();
        });
        
        this.filteredRequests = [...this.requests];
        this.updatePagination();
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

 

  getLeaveRequestTypeName(typeId: number): string {
    if (this.isLoadingTypes) {
      return 'Loading...';
    }
    
    const type = this.leaveRequestTypes.find(t => t.id === typeId);
    return type ? type.description || type.description : 'Unknown';
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

  filterRequests() {
    let results = this.requests;
    
    // Apply status filter
    if (this.statusFilter !== 'all') {
      results = results.filter(req => {
        switch (this.statusFilter) {
          case 'pending':
            return req.requestStatus === this.RequestStatus.PENDING;
          case 'approved':
            return req.requestStatus === this.RequestStatus.APPROVED;
          case 'rejected':
            return req.requestStatus === this.RequestStatus.REJECTED;
          default:
            return true;
        }
      });
    }
    
    // Apply search term
    if (this.searchTerm.trim()) {
      const term = this.searchTerm.toLowerCase().trim();
      results = results.filter(req => 
        req.reason?.toLowerCase().includes(term) ||
        req.id?.toString().includes(term) ||
        this.getLeaveRequestTypeName(req.leaveRequestTypeId).toLowerCase().includes(term)
      );
    }
    
    this.filteredRequests = results;
    this.currentPage = 1;
    this.updatePagination();
  }

  getRequestNumber(request: LeaveRequest): number {
    const index = this.filteredRequests.findIndex(r => r.id === request.id);
    return index + 1 + (this.currentPage - 1) * this.itemsPerPage;
  }

  updatePagination() {
    this.totalPages = Math.ceil(this.filteredRequests.length / this.itemsPerPage);
    
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    
    this.paginatedRequests = this.filteredRequests.slice(startIndex, endIndex);
  }

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.updatePagination();
    }
  }

  goToPreviousPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.updatePagination();
    }
  }

  goToNextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.updatePagination();
    }
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    
    if (this.totalPages <= maxVisible) {
      for (let i = 1; i <= this.totalPages; i++) {
        pages.push(i);
      }
    } else {
      if (this.currentPage <= 3) {
        for (let i = 1; i <= 4; i++) {
          pages.push(i);
        }
        pages.push(-1); 
        pages.push(this.totalPages);
      } else if (this.currentPage >= this.totalPages - 2) {
        pages.push(1);
        pages.push(-1);
        for (let i = this.totalPages - 3; i <= this.totalPages; i++) {
          pages.push(i);
        }
      } else {
        pages.push(1);
        pages.push(-1);
        for (let i = this.currentPage - 1; i <= this.currentPage + 1; i++) {
          pages.push(i);
        }
        pages.push(-1);
        pages.push(this.totalPages);
      }
    }
    
    return pages;
  }

  closeDetails() {
    this.selectedRequest = null;
    this.showListModal = true;
  }

}
