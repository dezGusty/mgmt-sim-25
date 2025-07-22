import { Component } from '@angular/core';
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

  constructor(private auth: Auth, private router: Router) { }

  onLogin() {
    this.auth.login(this.email, this.password).subscribe({
      next: (_) => {
        alert("Login successful!");
        this.auth.me().subscribe({
          next: (user: any) => {
            console.log('User info:', user); // Pentru debugging

            const role = user.role || user.Role; // Verifică ambele variante

            switch (role) {
              case 'Admin':
                this.router.navigate(['/admin']);
                break;
              case 'Manager':
                this.router.navigate(['/manager']);
                break;
              case 'Employee': // Rolul din backend
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
        console.error('Login error full:', err);
        console.error('Login error.error:', err.error);
        console.error('Login error.message:', err.message);

        let errorMessage = 'Unknown error';
        if (err.error?.message) {
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

  goBack() {
    this.router.navigate(['/']);
  }

  goToForgotPassword() {
    this.router.navigate(['/forgot-password']);
  }
}