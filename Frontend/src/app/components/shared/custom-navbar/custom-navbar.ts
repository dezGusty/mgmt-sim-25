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
  originalAdminEmail: string = ''; 
  showRoleDropdown = false;
  availableRoles: UserRole[] = [];
  isActingAsSecondManager = false;
  isTemporarilyReplaced = false;
  impersonatedUserName: string = '';
  impersonatedUserEmail: string = '';
  impersonatedRoles: string[] = [];
  impersonatedRolesDetailed: UserRole[] = [];
  
  originalAdminRoles: string[] = [];
  originalAdminRolesDetailed: UserRole[] = [];
  isCurrentlyImpersonating = false;

  constructor(private router: Router, private authService: Auth) { }

  ngOnInit(): void {
    this.loadUserData();
    
    // Subscribe to impersonation info changes
    this.authService.impersonation$.subscribe(info => {
      this.impersonatedUserName = info?.name || '';
      this.impersonatedRoles = info?.roles || [];
      this.impersonatedRolesDetailed = this.impersonatedRoles
        .map(r => this.mapRoleToInterface(r))
        .filter((r): r is UserRole => r !== null);
    });

    // Subscribe to impersonation state changes to trigger data reload
    this.authService.impersonationState$.subscribe(isImpersonating => {
      this.isCurrentlyImpersonating = isImpersonating;
      this.loadUserData(); // Reload data when impersonation state changes
    });
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event) {
    const target = event.target as HTMLElement;
    if (!target.closest('.role-dropdown-container')) {
      this.showRoleDropdown = false;
    }
  }

  toggleRoleDropdown(): void {
    if (this.userRoles.length > 1 || this.isCurrentlyImpersonating) {
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
      // Load current user data (impersonated user when impersonating, normal user otherwise)
      this.authService.me().subscribe({
        next: (data) => {
          if (data) {
            this.isActingAsSecondManager = data.isActingAsSecondManager || false;
            this.isTemporarilyReplaced = data.isTemporarilyReplaced || false;
            this.isCurrentlyImpersonating = data.isImpersonating || false;

            this.authService.updateCurrentUser(data);

            if (this.isCurrentlyImpersonating) {
              // When impersonating, the API returns impersonated user's email and data
              this.impersonatedUserEmail = data.email || '';
              this.userEmail = this.impersonatedUserEmail; // Show impersonated user email in main field
              
              // Load original admin email separately
              this.loadOriginalAdminEmail();
              
              // Show impersonated user's roles
              if (data.roles) {
                this.userRoles = data.roles;
                this.availableRoles = this.userRoles
                  .map(role => this.mapRoleToInterface(role))
                  .filter(role => role !== null) as UserRole[];
                
                this.showHomeButton = this.userRoles.length > 1;
              }
            } else {
              // Normal user - use their email and roles
              this.userEmail = data.email || '';
              this.originalAdminEmail = '';
              this.impersonatedUserEmail = '';
              
              if (data.roles) {
                this.userRoles = data.roles;
                this.showHomeButton = this.userRoles.length > 1;

                this.availableRoles = this.userRoles
                  .map(role => this.mapRoleToInterface(role))
                  .filter(role => role !== null) as UserRole[];
              }
            }

            console.log('[CustomNavbar] Loaded user data:', {
              email: this.userEmail,
              originalAdminEmail: this.originalAdminEmail,
              impersonatedEmail: this.impersonatedUserEmail,
              roles: this.userRoles,
              isImpersonating: this.isCurrentlyImpersonating
            });
          }
        },
        error: (error) => {
          console.error('Error loading user data:', error);
          this.showHomeButton = false;
          this.userEmail = '';
        }
      });
    } catch (error) {
      console.error('Error loading user data:', error);
      this.showHomeButton = false;
      this.userEmail = '';
    }
  }

  private loadOriginalAdminEmail(): void {
    // Load original admin email when impersonating
    this.authService.getOriginalUser().subscribe({
      next: (originalData) => {
        if (originalData) {
          this.originalAdminEmail = originalData.email || '';
          console.log('[CustomNavbar] Loaded original admin email:', this.originalAdminEmail);
        }
      },
      error: (error) => {
        console.error('Error loading original admin email:', error);
        this.originalAdminEmail = '';
      }
    });
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

  stopImpersonation(): void {
    if (confirm('Are you sure you want to stop impersonating this user? You will return to your admin account.')) {
      this.authService.stopImpersonation().subscribe({
        next: () => {
          this.authService.clearImpersonation();
          
          // Clear impersonation-related data
          this.isCurrentlyImpersonating = false;
          this.impersonatedUserName = '';
          this.impersonatedUserEmail = '';
          this.originalAdminEmail = '';
          this.impersonatedRoles = [];
          this.impersonatedRolesDetailed = [];
          this.originalAdminRoles = [];
          this.originalAdminRolesDetailed = [];
          
          // Reload user data to get back to original user's session
          this.loadUserData();
          this.router.navigate(['/role-selector']);
        },
        error: (err) => {
          console.error('Error stopping impersonation:', err);
          // Still clear local state even if backend call fails
          this.authService.clearImpersonation();
          
          this.isCurrentlyImpersonating = false;
          this.impersonatedUserName = '';
          this.impersonatedUserEmail = '';
          this.originalAdminEmail = '';
          this.impersonatedRoles = [];
          this.impersonatedRolesDetailed = [];
          this.originalAdminRoles = [];
          this.originalAdminRolesDetailed = [];
          
          this.loadUserData();
          this.router.navigate(['/role-selector']);
        }
      });
    }
  }
}
