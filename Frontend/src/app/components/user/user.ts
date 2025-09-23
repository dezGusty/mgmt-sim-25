import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserRequestForm } from './user-request-form/user-request-form';
import { UserLeaveBalance } from './user-leave-balance/user-leave-balance';
import { CustomNavbar } from '../shared/custom-navbar/custom-navbar';
import { ChatbotComponent } from '../chatbot/chatbot.component';
import { LeaveRequestService } from '../../services/leaveRequest/leaveRequest.service';
import { LeaveRequest } from '../../models/entities/iLeave-request';
import { ILeaveRequestType } from '../../models/entities/ileave-request-type';
import { LeaveRequestTypeService } from '../../services/leaveRequestType/leave-request-type-service';
import { RequestStatus } from '../../models/enums/RequestStatus';


@Component({
  selector: 'app-user',
  templateUrl: './user.html',
  styleUrl: './user.css',
  imports: [CommonModule, FormsModule, UserRequestForm, UserLeaveBalance, CustomNavbar, ChatbotComponent],
})
export class User {
  showRequestForm = false;
  showLeaveBalance = false;

  showSuccessMessage = false;
  successMessage = '';

  showCancelModal = false;
  requestToCancel: LeaveRequest | null = null;

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

  showCancelledRequests = false;

  leaveRequestTypes: ILeaveRequestType[] = [];
  isLoadingTypes = true;

  currentPage = 1;
  itemsPerPage = 5;
  totalPages = 0;
  totalCount = 0;
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

  getStatusForBackend(): string {
    if (!this.showCancelledRequests && this.statusFilter === 'all') {
      return 'ALL'; // Backend will handle excluding cancelled requests
    }
    
    switch (this.statusFilter) {
      case 'pending':
        return 'PENDING';
      case 'approved':
        return 'APPROVED';
      case 'rejected':
        return 'REJECTED';
      case 'all':
      default:
        return 'ALL';
    }
  }

  loadRequests() {
    this.isLoading = true;
    this.errorMessage = '';

    const status = this.getStatusForBackend();
    
    this.leaveRequestService.getCurrentUserLeaveRequestsPaginated(status, this.itemsPerPage, this.currentPage).subscribe({
      next: (data) => {       
        if (data.success && data.data) {
          this.paginatedRequests = data.data.items || [];
          this.totalPages = data.data.totalPages || 0;
          this.totalCount = data.data.totalCount || 0;
          // We no longer need filteredRequests since filtering is done on backend
          this.filteredRequests = this.paginatedRequests;
        } else {
          this.paginatedRequests = [];
          this.filteredRequests = [];
          this.totalPages = 0;
          this.totalCount = 0;
          this.errorMessage = data.message || 'Failed to load requests.';
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading requests:', err);
        this.isLoading = false;
        this.paginatedRequests = [];
        this.filteredRequests = [];
        this.totalPages = 0;
        this.totalCount = 0;

        if (err.status === 401) {
          this.errorMessage = 'You are not authorized. Please log in again.';
        } else if (err.status === 404) {
          this.errorMessage = '';  // Don't show error for no data found
        } else if (err.status === 400) {
          this.errorMessage = err.error?.message || 'Invalid request.';
        } else {
          this.errorMessage = err.error?.message || 'Failed to load requests.';
        }
      }
    })
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
    this.successMessage = 'Leave request submitted successfully!';
    
    this.showRequestForm = false;
    
    this.loadRequests();
    
    setTimeout(() => {
      this.showSuccessMessage = false;
    }, 5000);
  }

  closeSuccessMessage() {
    this.showSuccessMessage = false;
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
        case RequestStatus.CANCELED:
          return 'Canceled';
        default:
          return 'Canceled';
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
    return type ? type.title || type.title : 'Unknown';
  }

  cancelRequest(request: LeaveRequest) {
    this.requestToCancel = request;
    this.showCancelModal = true;
  }

  confirmCancelRequest() {
    if (!this.requestToCancel?.id) {
      return;
    }

    this.leaveRequestService.cancelLeaveRequestByEmployee(this.requestToCancel.id).subscribe({
      next: (response) => {
        this.loadRequests();

        if (this.selectedRequest && this.selectedRequest.id === this.requestToCancel?.id) {
          this.selectedRequest = null;
          this.showListModal = true;
        }

        this.showSuccessMessage = true;
        this.successMessage = 'Request cancelled successfully! 🚫';
        setTimeout(() => {
          this.showSuccessMessage = false;
        }, 3000);

        // Închide modalul
        this.closeCancelModal();
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to cancel request.';
        this.closeCancelModal();
      }
    });
  }

   closeCancelModal() {
    this.showCancelModal = false;
    this.requestToCancel = null;
  }
  
  filterRequests() {
    // Reset to first page and reload data with new filters
    this.currentPage = 1;
    this.loadRequests();
  }

  onShowCancelledToggle() {
    this.filterRequests();
  }

  getRequestNumber(request: LeaveRequest): number {
    const index = this.paginatedRequests.findIndex(r => r.id === request.id);
    return index + 1 + (this.currentPage - 1) * this.itemsPerPage;
  }


  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page;
      this.loadRequests();
    }
  }

  goToPreviousPage() {
    if (this.currentPage > 1) {
      this.goToPage(this.currentPage - 1);
    }
  }

  goToNextPage() {
    if (this.currentPage < this.totalPages) {
      this.goToPage(this.currentPage + 1);
    }
  }

  goToFirstPage() {
    this.goToPage(1);
  }

  goToLastPage() {
    this.goToPage(this.totalPages);
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
