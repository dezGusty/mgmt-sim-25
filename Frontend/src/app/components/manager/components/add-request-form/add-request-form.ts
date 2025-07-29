import { CommonModule } from '@angular/common';
import { Component, Output, EventEmitter, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LeaveRequestTypeService } from '../../../../services/leave-request-type';
import { EmployeeService } from '../../../../services/employee';
import { LeaveRequests } from '../../../../services/leave-requests/leave-requests';
import { FormUtils } from '../../../../utils/form.utils';

@Component({
  selector: 'app-add-request-form',
  imports: [CommonModule, FormsModule],
  templateUrl: './add-request-form.html',
  styleUrl: './add-request-form.css',
  providers: [LeaveRequestTypeService, EmployeeService, LeaveRequests],
})
export class AddRequestForm implements OnInit {
  showForm = true;
  userId: number | null = null;
  leaveRequestTypeId: number | null = null;
  startDate = '';
  endDate = '';
  reason = '';
  isSubmitting = false;
  today: string = FormUtils.getTodayDateString();
  showValidationErrors = false;

  leaveTypes: { id: number; description: string }[] = [];
  employees: { id: number; name: string }[] = [];

  @Output() close = new EventEmitter<void>();
  @Output() requestAdded = new EventEmitter<any>();

  constructor(
    private leaveTypeService: LeaveRequestTypeService,
    private employeeService: EmployeeService,
    private leaveRequests: LeaveRequests
  ) {}

  ngOnInit() {
    this.leaveTypeService.getLeaveTypes().subscribe((types) => {
      this.leaveTypes = types;
    });
    this.employeeService.getEmployees().subscribe((users) => {
      this.employees = users;
    });
  }

  handleSubmit() {
    if (
      !this.userId ||
      !this.leaveRequestTypeId ||
      !this.startDate ||
      !this.endDate
    ) {
      this.showValidationErrors = true;
      return;
    }

    this.isSubmitting = true;
    this.showValidationErrors = false;

    this.leaveRequests
      .addLeaveRequest({
        userId: this.userId,
        leaveRequestTypeId: this.leaveRequestTypeId,
        startDate: this.startDate,
        endDate: this.endDate,
        reason: this.reason || '',
      })
      .subscribe({
        next: (createdRequest) => {
          this.isSubmitting = false;
          console.log(
            'Request created and auto-approved successfully:',
            createdRequest
          );
          this.requestAdded.emit(createdRequest);
          this.handleClose();
        },
        error: (err) => {
          this.isSubmitting = false;
          console.error('Error creating or approving request:', err);
        },
      });
  }

  handleClose() {
    this.userId = null;
    this.leaveRequestTypeId = null;
    this.startDate = '';
    this.endDate = '';
    this.reason = '';
    this.showValidationErrors = false;
    this.close.emit();
  }
}
