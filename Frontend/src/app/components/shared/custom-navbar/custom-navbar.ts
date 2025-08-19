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
  userEmail: string = '';

  constructor(private router: Router, private authService: Auth) { }

  ngOnInit(): void {
    this.loadUserData();
  }

  getPrimaryRole(): string {
    if (this.userRoles.includes('Admin')) return 'Admin';
    if (this.userRoles.includes('HR')) return 'HR';
    if (this.userRoles.includes('Manager')) return 'Manager';
    if (this.userRoles.includes('Employee')) return 'Employee';
    return this.userRoles[0] || 'User';
  }

  getRoleColor(): string {
    const role = this.getPrimaryRole();
    switch (role) {
      case 'Admin':
        return 'bg-blue-600 text-white';
      case 'HR':
        return 'bg-purple-600 text-white ';
      case 'Manager':
        return 'bg-yellow-300 text-white';
      case 'Employee':
        return 'bg-green-600 text-white';
      default:
        return 'bg-gray-600 text-white';
    }
  }

  getRoleTextColor(): string {
    const role = this.getPrimaryRole();
    switch (role) {
      case 'Manager':
        return 'text-gray-900';
      default:
        return 'text-white';
    }
  }

  async loadUserData(): Promise<void> {
    try {
      const response = await fetch('https://localhost:7275/api/Auth/me', {
        credentials: 'include',
      });

      if (response.ok) {
        const data = await response.json();
        if (data) {
          this.userEmail = data.email || '';
          if (data.roles) {
            this.userRoles = data.roles;
            this.showHomeButton = this.userRoles.length > 1;
          }
        }
      }
    } catch (error) {
      console.error('Error loading user data:', error);
      this.showHomeButton = false;
      this.userEmail = '';
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

  goHome() {
    this.router.navigate(['/role-selector']);
  }
}
