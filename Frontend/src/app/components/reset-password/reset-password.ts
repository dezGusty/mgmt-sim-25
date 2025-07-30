import { Component } from '@angular/core';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Auth } from '../../services/authService/auth';

@Component({
  selector: 'app-reset-password',
  imports: [RouterModule, FormsModule, CommonModule],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.css'
})
export class ResetPassword {
  verificationCode: string = '';
  newPassword: string = '';
  confirmPassword: string = '';
  email: string = '';
  
  step: 'email' | 'reset' = 'email';
  loading = false;
  message: string = '';
  error: string = '';

  constructor(private authService: Auth, private router: Router) {}

  sendResetCode() {
    if(!this.email) {
      this.error = 'Please enter your email.';
      return;
    }

    this.loading = true;
    this.error = '';

    this.authService.sendResetCode(this.email).subscribe({
      next: (response) => {
        this.message = 'Reset code sent to your email';
        this.step = 'reset';
        this.loading = false;
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to send reset code';
        this.loading = false;
      }
    });
  }

  resetPassword() {
    if (!this.verificationCode || !this.newPassword || !this.confirmPassword) {
      this.error = 'All fields are required';
      return;
    }
    
    if (this.newPassword !== this.confirmPassword) {
      this.error = 'Passwords do not match';
      return;
    }
    
    this.loading = true;
    this.error = '';
    
    this.authService.resetPassword(this.verificationCode, this.newPassword, this.confirmPassword).subscribe({
      next: (response) => {
        this.message = 'Password reset successfully! Redirecting to login...';
        this.loading = false;
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      },
      error: (error) => {
        this.error = error.error?.message || 'Failed to reset password';
        this.loading = false;
      }
    });
  }

  goBack() {
    if (this.step === 'reset') {
      this.step = 'email';
      this.error = '';
      this.message = '';
    } else {
      this.router.navigate(['/login']);
    }
  }

}
