import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { UserRequestForm } from './user-request-form/user-request-form';
import { UserRequestsList } from './user-requests-list/user-requests-list';
import { UserLeaveBalance } from './user-leave-balance/user-leave-balance';
import { CustomNavbar } from '../shared/custom-navbar/custom-navbar';

@Component({
  selector: 'app-user',
  templateUrl: './user.html',
  styleUrl: './user.css',
  imports: [CommonModule, UserRequestForm, UserRequestsList, UserLeaveBalance, CustomNavbar],
})
export class User {
  showRequestForm = false;
  showRequestsList = false;
  showLeaveBalance = false;

  // ✅ Adăugați proprietăți pentru mesajul de succes
  showSuccessMessage = false;
  successMessage = '';

  constructor(private router: Router) {}

  goBack() {
    this.router.navigate(['/']);
  }
  
  toggleRequestForm() {
    this.showRequestForm = !this.showRequestForm;
  }

  toggleRequestsList() {
    this.showRequestsList = !this.showRequestsList;
  }

  toggleLeaveBalance() {
    this.showLeaveBalance = !this.showLeaveBalance;
  }

  onRequestSubmitted() {
    this.showSuccessMessage = true;
    this.successMessage = 'Leave request submitted successfully! 🎉';
    
    this.showRequestForm = false;
    
    if (this.showRequestsList) {
      this.showRequestsList = false; 
    }
    
    setTimeout(() => {
      this.showSuccessMessage = false;
    }, 5000);
  }

  closeSuccessMessage() {
    this.showSuccessMessage = false;
  }
}
