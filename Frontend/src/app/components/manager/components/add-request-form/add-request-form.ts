import { CommonModule } from '@angular/common';
import { Component, Output, EventEmitter, OnInit, OnDestroy, HostListener } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BehaviorSubject, Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { LeaveRequestTypeService } from '../../../../services/leaveRequestType/leave-request-type-service';
import { EmployeeService } from '../../../../services/employee';
import { LeaveRequests } from '../../../../services/leave-requests/leave-requests';
import { LeaveRequestService } from '../../../../services/leaveRequest/leaveRequest.service';
import { FormUtils } from '../../../../utils/form.utils';
import { ILeaveRequest } from '../../../../models/leave-request';
import { CreateLeaveRequestResponse } from '../../../../models/create-leave-request-response';
import { StatusUtils } from '../../../../utils/status.utils';
import { DateUtils } from '../../../../utils/date.utils';
import { Auth } from '../../../../services/authService/auth';
import { SecondManagerService } from '../../../../services/second-manager/second-manager.service';

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
    SecondManagerService,
  ],
})
export class AddRequestForm implements OnInit, OnDestroy {
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
  
  // Employee search properties
  employeeSearchTerm: string = '';
  filteredEmployees: { id: number; name: string }[] = [];
  showEmployeeDropdown: boolean = false;
  selectedEmployee: { id: number; name: string } | null = null;
  
  private employeeSearchSubject = new BehaviorSubject<string>('');
  private destroy$ = new Subject<void>();

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
    private leaveRequestService: LeaveRequestService,
    private authService: Auth,
    private secondManagerService: SecondManagerService
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
      this.filteredEmployees = users; // Initialize filtered employees
    });

    // Set up debounced employee search
    this.employeeSearchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(searchTerm => {
      this.filterEmployees(searchTerm);
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

  // Employee search methods
  onEmployeeSearchInput(event: any) {
    const searchTerm = event.target.value;
    this.employeeSearchTerm = searchTerm;
    this.employeeSearchSubject.next(searchTerm);
    this.showEmployeeDropdown = searchTerm.length > 0;
    
    // Clear selection if search term doesn't match selected employee
    if (this.selectedEmployee && !this.selectedEmployee.name.toLowerCase().includes(searchTerm.toLowerCase())) {
      this.selectedEmployee = null;
      this.userId = null;
      this.calculateRemainingDays();
    }
  }

  filterEmployees(searchTerm: string) {
    if (!searchTerm) {
      this.filteredEmployees = this.employees;
    } else {
      this.filteredEmployees = this.employees.filter(employee =>
        employee.name.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }
  }

  selectEmployee(employee: { id: number; name: string }) {
    this.selectedEmployee = employee;
    this.userId = employee.id;
    this.employeeSearchTerm = employee.name;
    this.showEmployeeDropdown = false;
    this.calculateRemainingDays();
  }

  closeEmployeeDropdown() {
    this.showEmployeeDropdown = false;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event) {
    // Close dropdown if clicking outside
    const target = event.target as HTMLElement;
    if (!target.closest('.employee-search-container')) {
      this.closeEmployeeDropdown();
    }
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

  async handleSubmit() {
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

    let reviewerId: number | undefined;

    // Dacă utilizatorul este manager secundar, obținem ID-ul managerului principal
    if (this.authService.isActingAsSecondManager()) {
      try {
        const response = await this.secondManagerService.getSecondManagerInfo().toPromise();
        if (response?.success && response.data) {
          reviewerId = response.data.managerId;
        }
      } catch (error) {
        console.error('Error getting primary manager ID:', error);
      }
    }

    this.leaveRequests
      .addLeaveRequest({
        userId: this.userId,
        leaveRequestTypeId: this.leaveRequestTypeId,
        startDate: this.startDate,
        endDate: this.endDate,
        reason: this.reason || '',
        reviewerId: reviewerId
      })
      .subscribe({
        next: (response) => {
          this.isSubmitting = false;
          console.log(
            'Request created and auto-approved successfully:',
            response
          );

          if (response.success && response.data) {
            const backendData = response.data;
            const status = StatusUtils.mapStatus(backendData.requestStatus);

            if (status !== undefined) {
              const formattedRequest: ILeaveRequest = {
                id: backendData.id.toString(),
                employeeName: backendData.fullName,
                status: status,
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

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    this.employeeSearchSubject.complete();
    
    if (this.calculationTimeout) {
      clearTimeout(this.calculationTimeout);
    }
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
    
    // Reset employee search
    this.employeeSearchTerm = '';
    this.selectedEmployee = null;
    this.showEmployeeDropdown = false;
    
    this.close.emit();
  }
}
