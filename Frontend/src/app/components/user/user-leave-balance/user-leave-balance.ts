import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LeaveRequestService } from '../../../services/leaveRequest/leaveRequest.service';
import { LeaveRequestTypeService } from '../../../services/leaveRequestType/leave-request-type-service';
import { ILeaveRequestType } from '../../../models/entities/ileave-request-type';

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
  
  leaveRequestTypes: ILeaveRequestType[] = [];
  selectedLeaveRequestTypeId: number = 0;
  isLoadingTypes = true;

  leaveCategories: LeaveCategory[] = [];
  upcomingLeaves: UpcomingLeave[] = [];
  isLoading = true;
  errorMessage = '';
  
  remainingDaysInfo: any | null = null;
  isLoadingRemainingDays = false;
  remainingDaysError = '';
  
  constructor(
    private leaveRequestService: LeaveRequestService,
    private leaveRequestTypeService: LeaveRequestTypeService
  ) {}

  ngOnInit() {
    this.loadLeaveRequestTypes();
    this.loadLeaveBalance();
  }

  loadLeaveRequestTypes() {
    console.log('Loading leave request types for balance view');
    this.isLoadingTypes = true;
    
    this.leaveRequestTypeService.getAllLeaveRequestTypes().subscribe({
      next: (types) => {
        console.log('Leave request types loaded for balance:', types);
        this.leaveRequestTypes = types.data;
        this.isLoadingTypes = false;
        
        // Auto-select first type if available
        if (types.data.length > 0) {
          this.selectedLeaveRequestTypeId = types.data[0].id;
          console.log('Auto-selected leave type:', this.selectedLeaveRequestTypeId);
          this.loadRemainingDaysForSelectedType();
        }
      },
      error: (err) => {
        console.log('Error loading leave request types for balance:', err);
        this.isLoadingTypes = false;
        this.errorMessage = 'Failed to load leave request types.';
      }
    });
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
  
  onLeaveTypeChange() {
    console.log('Leave type changed to:', this.selectedLeaveRequestTypeId);
    this.loadRemainingDaysForSelectedType(); // Va folosi anul curent selectat
  }

  onYearChange() {
    console.log('Year changed to:', this.selectedYear);
    this.loadRemainingDaysForSelectedType(); // Va folosi noul an selectat
  }

  loadRemainingDaysForSelectedType() {
    if (!this.selectedLeaveRequestTypeId || this.selectedLeaveRequestTypeId === 0) {
      console.log('No leave type selected');
      this.remainingDaysInfo = null;
      return;
    }

    const selectedType = this.leaveRequestTypes.find(type => type.id === this.selectedLeaveRequestTypeId);
    console.log('Loading remaining days for:');
    console.log('- Type ID:', this.selectedLeaveRequestTypeId);
    console.log('- Type Name:', selectedType?.title);
    console.log('- Type MaxDays (static):', selectedType?.maxDays);
    console.log('- Year:', this.selectedYear);

    this.isLoadingRemainingDays = true;
    this.remainingDaysError = '';

    this.leaveRequestService.getCurrentUserRemainingLeaveDays(
      this.selectedLeaveRequestTypeId, 
      this.selectedYear
    ).subscribe({
      next: (response) => {
        console.log('API Response for year', this.selectedYear, ':', response);
        console.log('API totalDays vs static maxDays:', response.data?.totalDays, 'vs', selectedType?.maxDays);
        this.remainingDaysInfo = response.data;
        this.isLoadingRemainingDays = false;
      },
      error: (err) => {
        console.log('Error loading remaining days for year', this.selectedYear, ':', err);
        console.log('Will fallback to static maxDays:', selectedType?.maxDays);
        this.isLoadingRemainingDays = false;
        this.remainingDaysInfo = null;
        this.remainingDaysError = 'Failed to load remaining days information.';
      }
    });
  }

  getTotalDays(): number {
    return this.leaveCategories.reduce((sum, category) => sum + category.total, 0);
  }

  getSelectedLeaveTypeMaxDays(): number {
    if (this.remainingDaysInfo && this.remainingDaysInfo.totalDays !== undefined) {
      console.log('Using totalDays from API:', this.remainingDaysInfo.totalDays);
      return this.remainingDaysInfo.totalDays;
    }
    
    const selectedType = this.leaveRequestTypes.find(type => type.id === this.selectedLeaveRequestTypeId);
    const maxDays = selectedType ? selectedType.maxDays : 0;
    console.log('Using maxDays from LeaveRequestType:', maxDays, 'for type:', selectedType?.title);
    return maxDays;
  }

  getSelectedLeaveTypeStaticMaxDays(): number {
    const selectedType = this.leaveRequestTypes.find(type => type.id === this.selectedLeaveRequestTypeId);
    return selectedType ? selectedType.maxDays : 0;
  }

  isUsingApiData(): boolean {
    return this.remainingDaysInfo && this.remainingDaysInfo.totalDays !== undefined;
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

  getSelectedLeaveTypeName(): string {
    const selectedType = this.leaveRequestTypes.find(type => type.id === this.selectedLeaveRequestTypeId);
    return selectedType ? selectedType.title : 'Select leave type';
  }

  getSelectedTypeUsedDays(): number {
    const totalDays = this.getSelectedLeaveTypeMaxDays();
    const remainingDays = this.getSelectedTypeRemainingDays();
    const usedDays = totalDays - remainingDays;
    
    console.log('Calculating used days:', totalDays, '-', remainingDays, '=', usedDays);
    return Math.max(0, usedDays); 
  }

  getSelectedTypeRemainingDays(): number {
    return this.remainingDaysInfo ? this.remainingDaysInfo.remainingDays : this.getSelectedLeaveTypeMaxDays();
  }

  getSelectedTypeUsagePercentage(): number {
    const totalDays = this.getSelectedLeaveTypeMaxDays();
    if (totalDays === 0) {
      return 0;
    }
    const usedDays = this.getSelectedTypeUsedDays();
    return (usedDays / totalDays) * 100;
  }
}