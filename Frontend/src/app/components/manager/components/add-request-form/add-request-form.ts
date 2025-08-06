import { CommonModule } from '@angular/common';
import { Component, Output, EventEmitter, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LeaveRequestTypeService } from '../../../../services/leaveRequestType/leave-request-type-service';
import { EmployeeService } from '../../../../services/employee';
import { LeaveRequests } from '../../../../services/leave-requests/leave-requests';
import { LeaveRequestService } from '../../../../services/leaveRequest/leaveRequest.service';
import { FormUtils } from '../../../../utils/form.utils';
import { ILeaveRequest } from '../../../../models/leave-request';
import { CreateLeaveRequestResponse } from '../../../../models/create-leave-request-response';
import { StatusUtils } from '../../../../utils/status.utils';
import { DateUtils } from '../../../../utils/date.utils';

@Component({
  selector: 'app-add-request-form',
  imports: [CommonModule, FormsModule],
  templateUrl: './add-request-form.html',
  styleUrl: './add-request-form.css',
  providers: [
    LeaveRequestTypeService,
    EmployeeService,
    LeaveRequests,
    LeaveRequestService,
  ],
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
  errorMessage = '';

  remainingDaysInfo: any = null;
  isCalculatingBalance = false;
  balanceCalculated = false;
  private calculationTimeout: any;

  leaveTypes: { id: number; title: string; isPaid: boolean }[] = [];
  employees: { id: number; name: string }[] = [];

  Math = Math;

  @Output() close = new EventEmitter<void>();
  @Output() requestAdded = new EventEmitter<any>();

  get canSubmitRequest(): boolean {
    if (
      !this.userId ||
      !this.leaveRequestTypeId ||
      !this.startDate ||
      !this.endDate
    ) {
      return false;
    }

    if (this.isCalculatingBalance) {
      return false;
    }

    if (!this.balanceCalculated) {
      return false;
    }

    return true;
  }

  getSelectedLeaveType() {
    return this.leaveTypes.find((type) => type.id === this.leaveRequestTypeId);
  }

  getSubmitButtonText(): string {
    if (this.isSubmitting) {
      return 'Adding Request...';
    }

    if (this.isCalculatingBalance) {
      return 'Calculating...';
    }

    if (
      !this.userId ||
      !this.leaveRequestTypeId ||
      !this.startDate ||
      !this.endDate
    ) {
      return 'Add Request';
    }

    if (!this.balanceCalculated) {
      return 'Validating...';
    }

    return 'Add Request';
  }

  constructor(
    private leaveTypeService: LeaveRequestTypeService,
    private employeeService: EmployeeService,
    private leaveRequests: LeaveRequests,
    private leaveRequestService: LeaveRequestService
  ) {}

  ngOnInit() {
    this.leaveTypeService.getLeaveTypes().subscribe((types) => {
      this.leaveTypes = types.map((type) => ({
        id: type.id,
        title: type.title,
        isPaid: type.isPaid,
      }));
    });
    this.employeeService.getEmployees().subscribe((users) => {
      this.employees = users;
    });
  }

  onStartDateChange() {
    if (this.endDate && this.startDate && this.endDate < this.startDate) {
      this.endDate = '';
    }
    this.errorMessage = '';
    this.calculateRemainingDays();
  }

  onEndDateChange() {
    this.errorMessage = '';
    this.calculateRemainingDays();
  }

  onUserChange() {
    this.calculateRemainingDays();
  }

  onLeaveTypeChange() {
    this.calculateRemainingDays();
  }

  calculateRemainingDays() {
    if (this.calculationTimeout) {
      clearTimeout(this.calculationTimeout);
    }

    this.remainingDaysInfo = null;
    this.balanceCalculated = false;

    if (
      !this.userId ||
      !this.leaveRequestTypeId ||
      !this.startDate ||
      !this.endDate
    ) {
      this.isCalculatingBalance = false;
      return;
    }

    this.isCalculatingBalance = true;

    this.calculationTimeout = setTimeout(() => {
      this.leaveRequestService
        .getRemainingLeaveDaysForPeriod(
          this.userId!,
          this.leaveRequestTypeId!,
          this.startDate,
          this.endDate
        )
        .subscribe({
          next: (response) => {
            this.isCalculatingBalance = false;
            this.balanceCalculated = true;
            if (response.success && response.data) {
              this.remainingDaysInfo = response.data;
            } else {
              this.remainingDaysInfo = null;
            }
          },
          error: (err) => {
            this.isCalculatingBalance = false;
            this.balanceCalculated = false;
            console.error('Error calculating remaining days:', err);
            this.remainingDaysInfo = null;
          },
        });
    }, 300);
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
    this.errorMessage = '';

    this.leaveRequests
      .addLeaveRequest({
        userId: this.userId,
        leaveRequestTypeId: this.leaveRequestTypeId,
        startDate: this.startDate,
        endDate: this.endDate,
        reason: this.reason || '',
      })
      .subscribe({
        next: (response) => {
          this.isSubmitting = false;
          console.log(
            'Request created and auto-approved successfully:',
            response
          );

          // Backend now returns all the data we need - create ILeaveRequest directly
          if (response.success && response.data) {
            const backendData = response.data;
            const formattedRequest: ILeaveRequest = {
              id: backendData.id.toString(),
              employeeName: backendData.fullName,
              status: StatusUtils.mapStatus(backendData.requestStatus),
              from: DateUtils.formatDate(backendData.startDate),
              to: DateUtils.formatDate(backendData.endDate),
              reason: backendData.reason,
              createdAt: DateUtils.formatDate(backendData.createdAt),
              comment: backendData.reviewerComment,
              createdAtDate: new Date(backendData.createdAt),
              departmentName: backendData.departmentName,
              leaveType: {
                title: backendData.leaveRequestTypeName,
                isPaid: backendData.leaveRequestTypeIsPaid,
              },
            };

            this.requestAdded.emit(formattedRequest);
          }

          this.handleClose();
        },
        error: (err) => {
          this.isSubmitting = false;
          console.error('Error creating or approving request:', err);

          if (err.status === 400 && err.error?.message) {
            this.errorMessage = err.error.message;
          } else {
            this.errorMessage =
              'An error occurred while creating the leave request. Please try again.';
          }
        },
      });
  }

  handleClose() {
    if (this.calculationTimeout) {
      clearTimeout(this.calculationTimeout);
    }

    this.userId = null;
    this.leaveRequestTypeId = null;
    this.startDate = '';
    this.endDate = '';
    this.reason = '';
    this.showValidationErrors = false;
    this.errorMessage = '';
    this.remainingDaysInfo = null;
    this.isCalculatingBalance = false;
    this.balanceCalculated = false;
    this.close.emit();
  }
}
