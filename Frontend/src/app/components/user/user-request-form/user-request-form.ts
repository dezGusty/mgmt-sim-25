import { Component, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LeaveRequestService } from '../../../services/leaveRequest/leaveRequest.service';
import { LeaveRequest } from '../../../models/entities/LeaveRequest';
import { LeaveRequestType } from '../../../models/entities/LeaveRequestType';
import { LeaveRequestTypeService } from '../../../services/leaveRequestType/leave-request-type';
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

  leaveRequestTypes: LeaveRequestType[] = [];
  isLoadingTypes = true;

  constructor(
    private leaveRequestService: LeaveRequestService, 
    private leaveRequestTypeService: LeaveRequestTypeService
  ) {
    console.log('📝 UserRequestForm component initialized');
  }

  ngOnInit() {
    this.loadLeaveRequestTypes();
  }

  loadLeaveRequestTypes() {
    console.log('📥 Loading leave request types from database...');
    this.isLoadingTypes = true;
    
    this.leaveRequestTypeService.getAllLeaveRequestTypes().subscribe({
      next: (types) => {
        console.log('✅ Leave request types loaded:', types);
        this.leaveRequestTypes = types;
        this.isLoadingTypes = false;
        
        if (types.length > 0) {
          this.leaveRequestTypeId = types[0].id;
        }
      },
      error: (err) => {
        console.error('❌ Error loading leave request types:', err);
        this.isLoadingTypes = false;
        this.errorMessage = 'Failed to load leave request types.';
      }
    });
  }
  
  closeForm() {
    console.log('❌ Closing request form');
    this.close.emit();
  }
  
  submitForm() {
    console.log('📤 Starting form submission...');
    console.log('📋 Form data:', {
      leaveRequestTypeId: this.leaveRequestTypeId,
      startDate: this.startDate,
      endDate: this.endDate,
      reason: this.reason
    });

    this.isSubmitting = true;
    this.errorMessage = '';
    
    const userId = 1; 
    console.log('👤 Using userId:', userId);
    
    const newRequest: LeaveRequest = {
      id: 0, 
      userId,
      leaveRequestTypeId: this.leaveRequestTypeId,
      startDate: new Date(this.startDate),
      endDate: new Date(this.endDate),
      reason: this.reason,
      requestStatus: RequestStatus.PENDING, 

    };
    
    console.log('📦 Prepared request object:', newRequest);
    console.log('🔗 Calling leaveRequestService.addLeaveRequest()...');
    
    this.leaveRequestService.addLeaveRequest(newRequest).subscribe({
      next: (createdRequest) => {
        console.log('✅ Request created successfully!', createdRequest);
        this.isSubmitting = false;
        this.requestSubmitted.emit(createdRequest);
        this.closeForm();
      },
      error: (err) => {
        console.error('❌ Error submitting leave request:', err);
        console.error('❌ Error details:', {
          message: err.message,
          status: err.status,
          statusText: err.statusText,
          error: err.error
        });
        this.isSubmitting = false;
        this.errorMessage = err.error?.message || 'A apărut o eroare la trimiterea cererii.';
      }
    });
  }
}