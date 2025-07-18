import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

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
  
  leaveCategories: LeaveCategory[] = [
    {
      name: 'Annual Leave',
      total: 21,
      used: 12,
      color: 'bg-blue-500',
    },
    {
      name: 'Sick Leave',
      total: 10,
      used: 3,
      color: 'bg-red-500',
    },
    {
      name: 'Work from Home',
      total: 30,
      used: 15,
      color: 'bg-green-500',
      note: 'Max 3 days per week'
    },
    {
      name: 'Special Leave',
      total: 5,
      used: 0,
      color: 'bg-purple-500',
      note: 'For special circumstances only'
    }
  ];
  
  upcomingLeaves: UpcomingLeave[] = [];
  
  ngOnInit() {
    // Simulate API call to fetch upcoming leaves
    this.upcomingLeaves = [
      {
        type: 'Annual Leave',
        startDate: new Date('2025-07-25'),
        endDate: new Date('2025-08-05'),
        days: 10,
        status: 'approved',
        typeColor: 'bg-blue-500'
      },
      {
        type: 'Work from Home',
        startDate: new Date('2025-07-20'),
        endDate: new Date('2025-07-20'),
        days: 1,
        status: 'pending',
        typeColor: 'bg-green-500'
      }
    ];
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