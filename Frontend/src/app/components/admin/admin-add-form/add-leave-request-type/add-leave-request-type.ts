import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LeaveRequestTypeService } from '../../../../services/leaveRequestType/leave-request-type-service';
import { ILeaveRequestType } from '../../../../models/entities/ileave-request-type';
import { IApiResponse } from '../../../../models/responses/iapi-response';

@Component({
  selector: 'app-add-leave-request-type',
  imports: [FormsModule, CommonModule],
  templateUrl: './add-leave-request-type.html',
  styleUrl: './add-leave-request-type.css',
})
export class AddLeaveRequestType {
  leaveTypeName: string = '';
  leaveTypeDescription: string = '';
  isPaid: boolean = false;

  onSubmitMessage: string = '';
  isSubmitting: boolean = false;

  constructor(private leaveRequestTypeService: LeaveRequestTypeService) {}

  isFieldInvalid(field: any): boolean {
    return field.invalid && (field.dirty || field.touched);
  }

  onSubmit(form: any) {
    if (form.valid) {
      this.isSubmitting = true;
      this.hideMessages();

      const lrt: ILeaveRequestType = {
        id: 0,
        description: this.leaveTypeName,
        title: this.leaveTypeDescription,
        isPaid: this.isPaid,
        maxDays: 0,
      };

      this.leaveRequestTypeService.postLeaveRequestType(lrt).subscribe({
        next: (response: IApiResponse<ILeaveRequestType>) => {
          this.isSubmitting = false;
          console.log('Leave type added successfully.');

          this.onSubmitMessage = 'Leave type added successfully.';
          this.onReset(form);
        },
        error: (error: any) => {
          this.isSubmitting = false;
          this.onSubmitMessage = 'Error adding a leave request type.';

          // Setează mesajul de eroare bazat pe răspunsul serverului
          if (error.error && error.error.message) {
            this.onSubmitMessage = error.error.message;
          } else if (error.message) {
            this.onSubmitMessage = error.message;
          } else {
            this.onSubmitMessage =
              'An error occurred while saving the leave type. Please try again.';
          }

          console.error('Error adding leave type:', error);
        },
      });
    }
  }

  onReset(form: any) {
    this.leaveTypeName = '';
    this.leaveTypeDescription = '';
    this.isPaid = false;
    this.hideMessages();
    form.resetForm();
  }

  hideMessages() {
    this.onSubmitMessage = '';
  }
}
