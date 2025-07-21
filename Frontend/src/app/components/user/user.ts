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
}
