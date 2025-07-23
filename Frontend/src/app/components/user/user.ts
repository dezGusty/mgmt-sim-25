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

  
  constructor(private router: Router) {
    console.log('🚀 User component initialized');
  }

  goBack() {
    console.log('📱 Navigating back to homepage');
    this.router.navigate(['/']);
  }
  
  toggleRequestForm() {
    console.log('🔄 Before toggle - showRequestForm:', this.showRequestForm);
    this.showRequestForm = !this.showRequestForm;
    console.log('🔄 After toggle - showRequestForm:', this.showRequestForm);
  }

  toggleRequestsList() {
    console.log('📋 Before toggle - showRequestsList:', this.showRequestsList);
    this.showRequestsList = !this.showRequestsList;
    console.log('📋 After toggle - showRequestsList:', this.showRequestsList);
  }

  toggleLeaveBalance() {
    console.log('⚖️ Before toggle - showLeaveBalance:', this.showLeaveBalance);
    this.showLeaveBalance = !this.showLeaveBalance;
    console.log('⚖️ After toggle - showLeaveBalance:', this.showLeaveBalance);
  }

  onRequestSubmitted() {
    console.log('✅ Leave request submitted successfully!');
    alert('The leave request has been submitted successfully!');
    // Refresh the requests list if it's open
    if (this.showRequestsList) {
      console.log('🔄 Refreshing requests list after submission');
    }
  }
}
