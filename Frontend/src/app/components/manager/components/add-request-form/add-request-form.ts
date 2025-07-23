import { CommonModule } from '@angular/common';
import { Component, Output, EventEmitter, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LeaveRequestTypeService } from '../../../../services/leave-request-type';
import { EmployeeService } from '../../../../services/employee';
import { LeaveRequests } from '../../../../services/leave-requests/leave-requests';

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

  leaveTypes: { id: number; description: string }[] = [];
  employees: { id: number; name: string }[] = [];

  @Output() close = new EventEmitter<void>();
  @Output() submit = new EventEmitter<{
    userId: number;
    leaveRequestTypeId: number;
    startDate: string;
    endDate: string;
    reason: string;
  }>();

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
      console.log('Employees:', users);
      this.employees = users;
    });
  }

  handleSubmit() {
    if (
      !this.userId ||
      !this.leaveRequestTypeId ||
      !this.startDate ||
      !this.endDate ||
      !this.reason
    )
      return;
    this.isSubmitting = true;
    this.leaveRequests
      .addLeaveRequest({
        userId: this.userId!,
        leaveRequestTypeId: this.leaveRequestTypeId!,
        startDate: this.startDate,
        endDate: this.endDate,
        reason: this.reason,
      })
      .subscribe({
        next: (res) => {
          this.isSubmitting = false;
          this.handleClose();
        },
        error: (err) => {
          this.isSubmitting = false;
          // poți adăuga aici un mesaj de eroare dacă vrei
        },
      });
  }

  handleClose() {
    this.userId = null;
    this.leaveRequestTypeId = null;
    this.startDate = '';
    this.endDate = '';
    this.reason = '';
    this.close.emit();
  }
}
