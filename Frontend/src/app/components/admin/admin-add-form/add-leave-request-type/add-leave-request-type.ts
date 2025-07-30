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

  // Proprietăți pentru mesaje
  showSuccessMessage: boolean = false;
  showErrorMessage: boolean = false;
  errorMessage: string = '';
  isSubmitting: boolean = false;

  constructor(private leaveRequestTypeService: LeaveRequestTypeService) {}

  isFieldInvalid(field: any): boolean {
    return field.invalid && (field.dirty || field.touched);
  }

  onSubmit(form: any) {
    if (form.valid) {
      this.isSubmitting = true;
      this.hideMessages(); // Ascunde mesajele anterioare

      const lrt: ILeaveRequestType = {
        id: 0,
        description: this.leaveTypeName,
        additionalDetails: this.leaveTypeDescription,
        isPaid: this.isPaid,
      };

      this.leaveRequestTypeService.postLeaveRequestType(lrt).subscribe({
        next: (response: IApiResponse<ILeaveRequestType>) => {
          this.isSubmitting = false;
          this.showSuccessMessage = true;
          this.showErrorMessage = false;
          console.log('Leave type added successfully');

          // Auto-hide success message după 5 secunde
          setTimeout(() => {
            this.showSuccessMessage = false;
          }, 5000);

          // Opțional: resetează formularul după succes
          this.onReset(form);
        },
        error: (error: any) => {
          this.isSubmitting = false;
          this.showErrorMessage = true;
          this.showSuccessMessage = false;

          // Setează mesajul de eroare bazat pe răspunsul serverului
          if (error.error && error.error.message) {
            this.errorMessage = error.error.message;
          } else if (error.message) {
            this.errorMessage = error.message;
          } else {
            this.errorMessage =
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
    this.showSuccessMessage = false;
    this.showErrorMessage = false;
    this.errorMessage = '';
  }

  dismissSuccessMessage() {
    this.showSuccessMessage = false;
  }

  dismissErrorMessage() {
    this.showErrorMessage = false;
  }
}
