import { Component, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LeaveRequestService } from '../../../services/leaveRequest/leaveRequest.service';
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

  get todayDate() {
    return new Date().toISOString().split('T')[0];
  }

  get minEndDate(): string {
    return this.startDate || this.todayDate;
  }

  constructor(
    private leaveRequestService: LeaveRequestService, 
    private leaveRequestTypeService: LeaveRequestTypeService
  ) {}

  ngOnInit() {
    this.loadLeaveRequestTypes();
  }

  onStartDateChange() {
    if (this.endDate && this.endDate < this.startDate) {
      console.log('⚠️ End date is before start date, resetting...');
      this.endDate = '';
    }
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
        }
      },
      error: (err) => {
        this.isLoadingTypes = false;
        this.errorMessage = 'Failed to load leave request types.';
      }
    });
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
  }

  closeForm() {
    this.close.emit();
  }
  
  submitForm() {

    if (!this.validateDates()) {
      return;
    }


    this.isSubmitting = true;
    this.errorMessage = '';
    
    const userId = 1; 
    console.log('Using userId:', userId);
    
    const newRequest: LeaveRequest = {
      id: 0, 
      userId,
      leaveRequestTypeId: this.leaveRequestTypeId,
      startDate: new Date(this.startDate),
      endDate: new Date(this.endDate),
      reason: this.reason,
      requestStatus: RequestStatus.PENDING, 
    };
    
    this.leaveRequestService.addLeaveRequest(newRequest).subscribe({
      next: (createdRequest) => {
        this.isSubmitting = false;
        
        this.requestSubmitted.emit(createdRequest.data);
        
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
        this.errorMessage = err.error?.message || 'An error occurred while submitting the request.';
      }
    });
  }
}