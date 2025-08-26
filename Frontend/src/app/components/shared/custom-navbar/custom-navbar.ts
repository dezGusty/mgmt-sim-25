import { Component, OnInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Auth } from '../../../services/authService/auth';

interface UserRole {
  name: string;
  displayName: string;
  description: string;
  route: string;
}

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
  showRoleDropdown = false;
  availableRoles: UserRole[] = [];
  isActingAsSecondManager = false;
  isTemporarilyReplaced = false;

  constructor(private router: Router, private authService: Auth) { }

  ngOnInit(): void {
    this.loadUserData();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event) {
    const target = event.target as HTMLElement;
    if (!target.closest('.role-dropdown-container')) {
      this.showRoleDropdown = false;
    }
  }

  toggleRoleDropdown(): void {
    if (this.userRoles.length > 1) {
      this.showRoleDropdown = !this.showRoleDropdown;
    }
  }

  switchRole(role: UserRole): void {
    this.showRoleDropdown = false;
    this.router.navigate([role.route]);
  }

  goToRoleSelector(): void {
    this.showRoleDropdown = false;
    this.router.navigate(['/role-selector']);
  }

  getRoleColor(roleName: string): string {
    switch (roleName) {
      case 'Admin':
        return 'bg-blue-600';
      case 'HR':
        return 'bg-purple-600';
      case 'Manager':
        return 'bg-yellow-500';
      case 'Employee':
        return 'bg-green-600';
      default:
        return 'bg-gray-600';
    }
  }

  private mapRoleToInterface(roleName: string): UserRole | null {
    const roleMapping: { [key: string]: UserRole } = {
      'Admin': {
        name: 'Admin',
        displayName: 'Administrator',
        description: 'Full system access and user management capabilities',
        route: '/admin'
      },
      'Manager': {
        name: 'Manager',
        displayName: 'Manager',
        description: 'Team management and leave request approvals',
        route: '/manager'
      },
      'Employee': {
        name: 'Employee',
        displayName: 'Employee',
        description: 'Submit and manage your leave requests',
        route: '/user'
      },
      'HR': {
        name: 'HR',
        displayName: 'Human Resources',
        description: 'Manage employee records and leave requests',
        route: '/hr'
      }
    };

    return roleMapping[roleName] || null;
  }

  getPrimaryRole(): string {
    if (this.userRoles.includes('Admin')) return 'Admin';
    if (this.userRoles.includes('HR')) return 'HR';
    if (this.userRoles.includes('Manager')) return 'Manager';
    if (this.userRoles.includes('Employee')) return 'Employee';
    return this.userRoles[0] || 'User';
  }

  getRoleColorForPrimary(): string {
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

  getPageColor(): string {
    const currentUrl = this.router.url;

    if (currentUrl.includes('/admin')) {
      return 'bg-blue-600';
    } else if (currentUrl.includes('/hr')) {
      return 'bg-purple-600';
    } else if (currentUrl.includes('/manager')) {
      return 'bg-yellow-500';
    } else if (currentUrl.includes('/user')) {
      return 'bg-green-600';
    } else if (currentUrl.includes('/role-selector')) {
      return 'bg-gray-400';
    } else {
      return 'bg-gray-600';
    }
  }

  getPageRole(): string {
    const currentUrl = this.router.url;

    if (currentUrl.includes('/admin')) {
      return 'Admin';
    } else if (currentUrl.includes('/hr')) {
      return 'HR';
    } else if (currentUrl.includes('/manager')) {
      return 'Manager';
    } else if (currentUrl.includes('/user')) {
      return 'Employee';
    } else if (currentUrl.includes('/role-selector')) {
      return '';
    } else {
      return this.getPrimaryRole();
    }
  }

  isRoleSelectorPage(): boolean {
    return this.router.url.includes('/role-selector');
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
          this.isActingAsSecondManager = data.isActingAsSecondManager || false;
          this.isTemporarilyReplaced = data.isTemporarilyReplaced || false;

          this.authService.updateCurrentUser(data);

          if (data.roles) {
            this.userRoles = data.roles;
            this.showHomeButton = this.userRoles.length > 1;

            this.availableRoles = this.userRoles
              .map(role => this.mapRoleToInterface(role))
              .filter(role => role !== null) as UserRole[];
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
