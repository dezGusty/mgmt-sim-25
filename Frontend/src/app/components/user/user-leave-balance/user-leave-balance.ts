import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LeaveRequestService } from '../../../services/leaveRequest/leaveRequest.service';

interface LeaveCategory {
  name: string;
  total: number;
  used: number;
  color: string;
  note?: string;
}

interface UpcomingLeave {
  type: string;
  startDate: Date;
  endDate: Date;
  days: number;
  status: 'pending' | 'approved';
  typeColor: string;
}

@Component({
  selector: 'app-user-leave-balance',
  imports: [CommonModule, FormsModule],
  templateUrl: './user-leave-balance.html',
  styleUrl: './user-leave-balance.css',
})
export class UserLeaveBalance implements OnInit {
  @Output() close = new EventEmitter<void>();

  selectedYear = new Date().getFullYear();
  availableYears = [2023, 2024, 2025, 2026];

  leaveCategories: LeaveCategory[] = [];
  upcomingLeaves: UpcomingLeave[] = [];
  isLoading = true;
  errorMessage = '';
  
  constructor(private leaveRequestService: LeaveRequestService) {}

  
  ngOnInit() {
    this.loadLeaveBalance();
  }

  loadLeaveBalance() {
    this.isLoading = true;
    const userId = 1; 
    
    this.leaveRequestService.getLeaveBalance(userId).subscribe({
      next: (data) => {
        this.leaveCategories = data.categories;
        this.upcomingLeaves = data.upcomingLeaves;
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = 'Nu s-a putut încărca balanța de concediu.';
        this.isLoading = false;
        console.error('Error loading leave balance:', err);
      }
    });
  }
  
  getTotalDays(): number {
    return this.leaveCategories.reduce((sum, category) => sum + category.total, 0);
  }
  
  getUsedDays(): number {
    return this.leaveCategories.reduce((sum, category) => sum + category.used, 0);
  }
  
  getRemainingDays(): number {
    return this.getTotalDays() - this.getUsedDays();
  }
  
  getUsagePercentage(): number {
    return (this.getUsedDays() / this.getTotalDays()) * 100;
  }
}