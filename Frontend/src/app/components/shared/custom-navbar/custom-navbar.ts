import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Auth } from '../../../services/authService/auth';

@Component({
  selector: 'custom-navbar',
  imports: [CommonModule],
  templateUrl: './custom-navbar.html',
})
export class CustomNavbar implements OnInit {
  showAddRequestForm = false;
  userRoles: string[] = [];
  showHomeButton = true;

  constructor(private router: Router, private authService: Auth) {}

  ngOnInit(): void {
    this.loadUserRoles();
  }

  async loadUserRoles(): Promise<void> {
    try {
      const response = await fetch('https://localhost:7275/api/Auth/me', {
        credentials: 'include',
      });

      if (response.ok) {
        const data = await response.json();
        if (data && data.roles) {
          this.userRoles = data.roles;
          this.showHomeButton = this.userRoles.length > 1;
        }
      }
    } catch (error) {
      console.error('Error loading user roles:', error);
      this.showHomeButton = false;
    }
  }

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
