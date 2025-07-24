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
    this.auth.login(this.email, this.password).subscribe({
      next: (_) => {
        this.auth.me().subscribe({
          next: (user: any) => {
            const role = user.role || user.Role;

            switch (role) {
              case 'Admin':
                this.router.navigate(['/admin']);
                break;
              case 'Manager':
                this.router.navigate(['/manager']);
                break;
              case 'Employee':
                this.router.navigate(['/user']);
                break;
              default:
                console.warn('Unknown role:', role);
                this.router.navigate(['/']);
                break;
            }
          },
          error: (err) => {
            console.error('Failed to get user info:', err);
            alert("Failed to retrieve user information.");
            this.router.navigate(['/login']);
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

  onSubmit() {
    this.auth.login(this.email, this.password).subscribe({
      next: () => this.router.navigate(['/manager']),
      error: (err) => {
        this.errorMessage = 'Invalid credentials';
        console.error(err);
      },
    });
  }
}