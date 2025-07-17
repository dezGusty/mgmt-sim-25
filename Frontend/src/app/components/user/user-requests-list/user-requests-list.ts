import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface LeaveRequest {
  id: number;
  type: string;
  startDate: Date;
  endDate: Date;
  duration: number;
  status: 'pending' | 'approved' | 'rejected';
  reason: string;
  managerComment?: string;
  selected?: boolean;
}

@Component({
  selector: 'app-user-requests-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './user-requests-list.html',
  styleUrl: './user-requests-list.css',
})
export class UserRequestsList implements OnInit {
  @Output() close = new EventEmitter<void>();
  
  requests: LeaveRequest[] = [];
  filteredRequests: LeaveRequest[] = [];
  selectedRequest: LeaveRequest | null = null;
  isLoading = true;
  statusFilter: 'all' | 'pending' | 'approved' | 'rejected' = 'all';
  searchTerm = '';
  showListModal = true;

  ngOnInit() {
    // Simulate API call to fetch data
    setTimeout(() => {
      this.requests = [
        {
          id: 1001,
          type: 'Annual Leave',
          startDate: new Date('2025-07-25'),
          endDate: new Date('2025-08-05'),
          duration: 10,
          status: 'approved',
          reason: 'Summer vacation with family',
          managerComment: 'Approved. Have a great vacation!'
        },
        {
          id: 1002,
          type: 'Sick Leave',
          startDate: new Date('2025-06-10'),
          endDate: new Date('2025-06-12'),
          duration: 3,
          status: 'approved',
          reason: 'Down with flu'
        },
        {
          id: 1003,
          type: 'Work from Home',
          startDate: new Date('2025-07-20'),
          endDate: new Date('2025-07-20'),
          duration: 1,
          status: 'pending',
          reason: 'Apartment maintenance scheduled'
        },
        {
          id: 1004,
          type: 'Annual Leave',
          startDate: new Date('2024-12-27'),
          endDate: new Date('2025-01-03'),
          duration: 6,
          status: 'rejected',
          reason: 'Winter holiday',
          managerComment: 'Sorry, we have critical deadlines during this period.'
        }
      ];
      
      this.filterRequests();
      this.isLoading = false;
    }, 800);
  }

  setStatusFilter(status: 'all' | 'pending' | 'approved' | 'rejected') {
    this.statusFilter = status;
    this.filterRequests();
  }

  filterRequests() {
    let results = this.requests;
    
    // Apply status filter
    if (this.statusFilter !== 'all') {
      results = results.filter(req => req.status === this.statusFilter);
    }
    
    // Apply search term
    if (this.searchTerm.trim()) {
      const term = this.searchTerm.toLowerCase().trim();
      results = results.filter(req => 
        req.type.toLowerCase().includes(term) || 
        req.reason.toLowerCase().includes(term) ||
        req.id.toString().includes(term)
      );
    }
    
    this.filteredRequests = results;
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
      // In a real app, this would be an API call
      const index = this.requests.findIndex(r => r.id === request.id);
      if (index !== -1) {
        this.requests.splice(index, 1);
        this.filterRequests();
        
        if (this.selectedRequest && this.selectedRequest.id === request.id) {
          this.selectedRequest = null;
          this.showListModal = true;
        }
      }
    }
  }
}