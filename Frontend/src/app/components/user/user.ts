import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { UserRequestForm } from './user-request-form/user-request-form';

@Component({
  selector: 'app-user',
  templateUrl: './user.html',
  styleUrl: './user.css',
  imports: [CommonModule, UserRequestForm],
})
export class User {
  showRequestForm = false;
  
  constructor(private router: Router) {}

  goBack() {
    this.router.navigate(['/']);
  }
  
  toggleRequestForm() {
    this.showRequestForm = !this.showRequestForm;
  }
}
