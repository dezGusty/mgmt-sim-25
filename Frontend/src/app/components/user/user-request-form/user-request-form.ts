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
  
  // Model pentru formular
  fromDate = '';
  toDate = '';
  reason = '';
  type = 'annual'; // Tipul de concediu (annual, medical, unpaid, etc.)
  isSubmitting = false;
  
  // Metoda pentru închiderea formularului
  closeForm() {
    this.close.emit();
  }
  
  // Metoda pentru trimiterea formularului
  submitForm() {
    this.isSubmitting = true;
    
    // Aici ar veni logica de trimitere către server
    // Simulăm o cerere asincronă
    setTimeout(() => {
      this.isSubmitting = false;
      // După trimitere, închidem formularul
      this.closeForm();
      
      // În aplicația reală, aici am salva datele și am actualiza UI-ul
    }, 1000);
  }
}
