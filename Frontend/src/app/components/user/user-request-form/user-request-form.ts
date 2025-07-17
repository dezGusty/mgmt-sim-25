import { Component, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-user-request-form',
  templateUrl: './user-request-form.html',
  styleUrl: './user-request-form.css',
  imports: [CommonModule, FormsModule],
})
export class UserRequestForm {
  @Output() close = new EventEmitter<void>();
  
  fromDate = '';
  toDate = '';
  reason = '';
  type = 'annual';
  isSubmitting = false;
  
  closeForm() {
    this.close.emit();
  }
  
  submitForm() {
    this.isSubmitting = true;
    
    // Here would be the logic to send the request to the server
    // Simulating an asynchronous request
    setTimeout(() => {
      this.isSubmitting = false;
      this.closeForm();
      
    }, 1000);
  }
}
