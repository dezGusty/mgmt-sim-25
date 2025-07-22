import { CommonModule } from '@angular/common';
import { Component, Output, EventEmitter } from '@angular/core';

import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-add-request-form',
  imports: [CommonModule, FormsModule],
  templateUrl: './add-request-form.html',
  styleUrl: './add-request-form.css',
})
export class AddRequestForm {
  showForm = true;
  employee = '';
  from = '';
  to = '';
  reason = '';
  isSubmitting = false;

  @Output() close = new EventEmitter<void>();
  @Output() submit = new EventEmitter<{
    employee: string;
    from: string;
    to: string;
    reason: string;
  }>();

  handleSubmit() {
    if (!this.employee || !this.from || !this.to || !this.reason) return;
    this.isSubmitting = true;
    setTimeout(() => {
      this.submit.emit({
        employee: this.employee,
        from: this.from,
        to: this.to,
        reason: this.reason,
      });
      this.isSubmitting = false;
      this.handleClose();
    }, 500);
  }

  handleClose() {
    this.employee = '';
    this.from = '';
    this.to = '';
    this.reason = '';
    this.close.emit();
  }
}
