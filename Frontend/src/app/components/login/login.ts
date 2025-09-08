import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { Auth } from '../../services/authService/auth';

import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AnimatedBackground } from '../shared/animated-background/animated-background';

@Component({
  selector: 'app-login',
  imports: [RouterModule, CommonModule, FormsModule, AnimatedBackground],
  templateUrl: './login.html',
  styleUrls: ['./login.css'],
})
export class Login {

  email = '';
  password = '';
  errorMessage = '';

  welcomeAnimate = false;
  welcomeName = '';

  constructor(private auth: Auth, private router: Router) { }

  onLogin() {
    this.errorMessage = '';
    const url = new URL(window.location.href);
    const returnUrl = url.searchParams.get('returnUrl') || undefined;

    this.auth.login(this.email, this.password).subscribe({
      next: (_) => {
        this.auth.me().subscribe({
          next: (user: any) => {
            this.auth.updateCurrentUser(user);
            const firstName = user.firstName || user.FirstName || '';
            const lastName = user.lastName || user.LastName || '';
            this.welcomeName = `${firstName} ${lastName}`.trim() || user.email || user.Email || '';
            this.welcomeAnimate = true;
            
            const continueNav = () => {
              if (returnUrl) {
                this.router.navigateByUrl(returnUrl);
                return;
              }
            if (user && user.roles && user.roles.length === 1) {
              const singleRole = user.roles[0];
              this.redirectToRoleSpecificPage(singleRole);
            } else if (user && user.roles && user.roles.length > 1) {
              this.router.navigate(['/role-selector']);
            } else {
              this.router.navigate(['/role-selector']);
            }
            };

            setTimeout(continueNav, 1200);
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
        this.errorMessage = errorMessage;
        setTimeout(() => {
          this.errorMessage = '';
        }, 5000);
      }
    });
  }

  private redirectToRoleSpecificPage(role: string, returnUrl?: string): void {
    console.log('Redirecting for single role:', role); 
    
    if (returnUrl) {
      this.router.navigateByUrl(returnUrl);
      return;
    }

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