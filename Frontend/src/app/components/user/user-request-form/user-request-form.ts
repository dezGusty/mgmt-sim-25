import { Component, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CreateLeaveRequestByEmployeeDto, LeaveRequestService, RemainingLeaveDaysResponse } from '../../../services/leaveRequest/leaveRequest.service';
import { LeaveRequest } from '../../../models/entities/iLeave-request';
import { ILeaveRequestType } from '../../../models/entities/ileave-request-type';
import { LeaveRequestTypeService } from '../../../services/leaveRequestType/leave-request-type-service';
import { RequestStatus } from '../../../models/enums/RequestStatus';

@Component({
  selector: 'app-user-request-form',
  templateUrl: './user-request-form.html',
  styleUrl: './user-request-form.css',
  imports: [CommonModule, FormsModule],
})
export class UserRequestForm {
  @Output() close = new EventEmitter<void>();
  @Output() requestSubmitted = new EventEmitter<LeaveRequest>();

  startDate = '';
  endDate = '';
  reason = '';
  leaveRequestTypeId: number = 0;
  isSubmitting = false;
  errorMessage = '';

  leaveRequestTypes: ILeaveRequestType[] = [];
  isLoadingTypes = true;

  remainingDaysInfo: any | null = null;
  isLoadingRemainingDays = false;
  remainingDaysError = '';
  balanceCalculated = false;


  Math = Math;

  get todayDate() {
    return new Date().toISOString().split('T')[0];
  }

  get minEndDate(): string {
    return this.startDate || this.todayDate;
  }

  constructor(
    private leaveRequestService: LeaveRequestService,
    private leaveRequestTypeService: LeaveRequestTypeService
  ) { }


  ngOnInit() {
    this.loadLeaveRequestTypes();
  }

  onStartDateChange() {
    this.resetErrorState();

    if (this.endDate && this.endDate < this.startDate) {
      this.endDate = '';
    }
    this.triggerRemainingDaysUpdate();
  }

  onEndDateChange() {
    this.resetErrorState();
    this.triggerRemainingDaysUpdate();
  }

  onLeaveTypeChange() {
    this.resetErrorState();
    this.triggerRemainingDaysUpdate();
  }

  private resetErrorState() {
    this.errorMessage = '';
    this.remainingDaysInfo = null;
    this.balanceCalculated = false;
    this.remainingDaysError = '';
  }

  private triggerRemainingDaysUpdate() {
    if (this.leaveRequestTypeId && this.leaveRequestTypeId > 0 && this.startDate && this.endDate) {
      this.updateRemainingDays();
    } else {
      this.remainingDaysInfo = null;
      this.remainingDaysError = '';
      this.balanceCalculated = false;
    }
  }

  private updateRemainingDays() {
    if (!this.leaveRequestTypeId || this.leaveRequestTypeId === 0 || !this.startDate || !this.endDate) {
      return;
    }

    this.isLoadingRemainingDays = true;
    this.remainingDaysError = '';
    this.balanceCalculated = false;

    this.leaveRequestService.getCurrentUserRemainingLeaveDaysForPeriod(
      this.leaveRequestTypeId,
      this.startDate,
      this.endDate
    ).subscribe({
      next: (response) => {
        this.remainingDaysInfo = response.data;
        this.isLoadingRemainingDays = false;
        this.balanceCalculated = true;
      },
      error: (err) => {
        this.isLoadingRemainingDays = false;
        this.remainingDaysInfo = null;
        this.remainingDaysError = 'Failed to load remaining days information.';
        this.balanceCalculated = false;
        console.error('Error loading remaining days:', err);
      }
    });
  }

  validateDates(): boolean {
    const today = new Date().toISOString().split('T')[0];

    if (this.startDate < today) {
      this.errorMessage = 'Start date cannot be in the past.';
      return false;
    }

    if (this.endDate < this.startDate) {
      this.errorMessage = 'End date cannot be before start date.';
      return false;
    }

    if (!this.startDate || !this.endDate) {
      this.errorMessage = 'Please select both start and end dates.';
      return false;
    }

    return true;
  }

  loadLeaveRequestTypes() {
    this.isLoadingTypes = true;

    this.leaveRequestTypeService.getAllLeaveRequestTypes().subscribe({
      next: (types) => {
        this.leaveRequestTypes = types.data;
        this.isLoadingTypes = false;

        if (types.data.length > 0) {
          this.leaveRequestTypeId = types.data[0].id;
          this.triggerRemainingDaysUpdate();
        }
      },
      error: (err) => {
        this.isLoadingTypes = false;
        this.errorMessage = 'Failed to load leave request types.';
      }
    });
  }

  getSelectedLeaveTypeName(): string {
    const selectedType = this.leaveRequestTypes.find(type => type.id === this.leaveRequestTypeId);
    return selectedType ? selectedType.title : '';
  }

  resetFormSmooth() {
    const formElement = document.querySelector('.form-container');
    if (formElement) {
      formElement.classList.add('fade-out');
    }

    setTimeout(() => {
      this.startDate = '';
      this.endDate = '';
      this.reason = '';
      this.leaveRequestTypeId = this.leaveRequestTypes.length > 0 ? this.leaveRequestTypes[0].id : 0;
      this.errorMessage = '';
      this.remainingDaysInfo = null;
      this.remainingDaysError = '';
      this.balanceCalculated = false;

      if (formElement) {
        formElement.classList.remove('fade-out');
        formElement.classList.add('fade-in');

        setTimeout(() => {
          formElement.classList.remove('fade-in');
        }, 300);
      }
    }, 150);
  }

  resetForm() {
    this.startDate = '';
    this.endDate = '';
    this.reason = '';
    this.leaveRequestTypeId = this.leaveRequestTypes.length > 0 ? this.leaveRequestTypes[0].id : 0;
    this.errorMessage = '';
    this.remainingDaysInfo = null;
    this.remainingDaysError = '';
    this.balanceCalculated = false;
  }

  closeForm() {
    this.close.emit();
  }

  submitForm() {
    if (!this.validateDates()) {
      return;
    }

    if (!this.leaveRequestTypeId || this.leaveRequestTypeId === 0) {
      this.errorMessage = 'Please select a leave request type.';
      return;
    }

    // Additional validation: check if user has enough remaining days
    if (this.remainingDaysInfo && this.remainingDaysInfo.remainingDays < 0) {
      this.errorMessage = 'You do not have enough remaining days for this leave type.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const requestDto: CreateLeaveRequestByEmployeeDto = {
      leaveRequestTypeId: this.leaveRequestTypeId,
      startDate: this.startDate,
      endDate: this.endDate,
      reason: this.reason || undefined,
      requestStatus: RequestStatus.PENDING,
    };

    this.leaveRequestService.addLeaveRequestByEmployee(requestDto).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        this.requestSubmitted.emit(response.data);

        setTimeout(() => {
          this.resetFormSmooth();
        }, 300);

        setTimeout(() => {
          this.closeForm();
        }, 800);
      },
      error: (err) => {
        console.error('Error details:', {
          message: err.message,
          status: err.status,
          statusText: err.statusText,
          error: err.error
        });
        this.isSubmitting = false;

        if (err.status === 401) {
          this.errorMessage = 'You are not authorized. Please log in again.';
        } else if (err.status === 400) {
          this.errorMessage = err.error?.message || 'Invalid request data.';
        } else if (err.status === 404) {
          this.errorMessage = 'Service not found. Please contact support.';
        } else {
          this.errorMessage = err.error?.message || 'An error occurred while submitting the request.';
        }
      }
    });
  }
}