import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Auth } from '../../../services/authService/auth';

@Component({
  selector: 'custom-navbar',
  imports: [CommonModule],
  templateUrl: './custom-navbar.html',
})
export class CustomNavbar {
  showAddRequestForm = false;

  constructor(private router: Router, private authService: Auth) {}

  goBack() {
    this.router.navigate(['/']);
  }

  logout() {
    this.authService.logout().subscribe({
      next: () => {
        this.router.navigate(['/login']);
      },
      error: (error) => {
        console.error('Logout failed:', error);
        this.router.navigate(['/login']);
      },
    });
  }

  goHome(){
    this.router.navigate(['/role-selector']);
  }
}
