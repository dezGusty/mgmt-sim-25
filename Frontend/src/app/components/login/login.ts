import { Component, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { Auth } from '../../services/authService/auth';

import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  imports: [RouterModule, CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css'],
})
export class Login {

  email = '';
  password = '';
  errorMessage = '';

  constructor(private auth: Auth, private router: Router) { }

  onLogin() {
    this.errorMessage = '';
    this.auth.login(this.email, this.password).subscribe({
      next: (_) => {
        this.auth.me().subscribe({
          next: (user: any) => {
            console.log('User data:', user);
            
            if (user && user.roles && user.roles.length === 1) {
              const singleRole = user.roles[0];
              this.redirectToRoleSpecificPage(singleRole);
            } else if (user && user.roles && user.roles.length > 1) {
              this.router.navigate(['/role-selector']);
            } else {
              this.router.navigate(['/role-selector']);
            }
          },
          error: (err) => {
            console.error('Error getting user info:', err);
            this.router.navigate(['/role-selector']);
          }
        });
      },
      error: (err) => {
        let errorMessage = 'Unable to log in. Please check your credentials.';
        if (err.status === 401) {
          errorMessage = 'Unauthorized access - please log in with a valid account.';
        } else if (err.error?.message) {
          errorMessage = err.error.message;
        } else if (err.error) {
          errorMessage = err.error;
        } else if (err.message) {
          errorMessage = err.message;
        }
        alert("Login failed: " + errorMessage);
      }
    });
  }

  private redirectToRoleSpecificPage(role: string): void {
    console.log('Redirecting for single role:', role); 
    
    switch (role.toLowerCase()) {
      case 'admin':
        this.router.navigate(['/admin']);
        break;
      case 'manager':
        this.router.navigate(['/manager']);
        break;
      case 'employee':
        this.router.navigate(['/user']);
        break;
      default:
        console.warn('Unknown role:', role);
        this.router.navigate(['/role-selector']);
        break;
    }
  }
}