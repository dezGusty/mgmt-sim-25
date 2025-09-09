import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Auth } from '../../../services/authService/auth';
import { CustomNavbar } from '../custom-navbar/custom-navbar';
import { AnimatedBackground } from '../animated-background/animated-background';


interface UserRole {
  userId?: number;
  name: string;
  displayName: string;
  description: string;
  icon: string;
  route: string;
  color: string;
  bgGradient: string;
}

@Component({
  selector: 'app-role-selector',
  imports: [CommonModule, CustomNavbar, AnimatedBackground],
  templateUrl: './role-selector.html',
  styleUrls: ['./role-selector.css'],
})
export class RoleSelector implements OnInit {
  userRoles: UserRole[] = [];
  impersonatedRoles: UserRole[] = [];
  adminOriginalRoles: UserRole[] = [];
  userName = '';
  impersonatedUserName = '';
  isImpersonating = false;
  isLoading = true;
  errorMessage = '';

  constructor(
    private router: Router,
    private auth: Auth
  ) { }

  ngOnInit() {
    this.loadUserRoles();
    this.auth.impersonation$.subscribe(info => {
      this.impersonatedUserName = info?.name || '';
      this.isImpersonating = !!info;
      if (info?.roles) {
        this.impersonatedRoles = info.roles.map(role => this.mapRoleToInterface(role))
          .filter((role): role is UserRole => role !== null);
      } else {
        this.impersonatedRoles = [];
      }
    });
  }

  loadUserRoles() {
    this.isLoading = true;
    this.errorMessage = '';

    this.auth.me().subscribe({
      next: (user: any) => {
        this.auth.updateCurrentUser(user);

        this.userName = `${user.firstName || user.FirstName || ''} ${user.lastName || user.LastName || ''}`.trim()
          || user.email || user.Email || 'User';

        // Extract roles - handle both normal user and impersonated user scenarios
        const userRoles = user.Roles || user.roles || [];
        const originalRoles = user.OriginalRoles || user.originalRoles || [];
        const isImpersonating = user.IsImpersonating || user.isImpersonating;
        
        if (isImpersonating) {
          // When impersonating, userRoles should only contain the impersonated user's roles
          this.userRoles = userRoles.map((role: string) => this.mapRoleToInterface(role))
            .filter((role: UserRole | null) => role !== null);
            
          // Admin's original roles are shown separately and require stopping impersonation
          this.adminOriginalRoles = originalRoles.map((role: string) => this.mapRoleToInterface(role))
            .filter((role: UserRole | null) => role !== null);
        } else {
          // When not impersonating, show the user's normal roles
          this.userRoles = userRoles.map((role: string) => this.mapRoleToInterface(role))
            .filter((role: UserRole | null) => role !== null);
          this.adminOriginalRoles = [];
        }

        this.isLoading = false;

        // Handle cases where there are no roles
        if (this.userRoles.length === 0 && !isImpersonating) {
          this.errorMessage = 'No accessible roles found for your account. Please contact your administrator.';
        } else if (this.userRoles.length === 0 && isImpersonating && this.adminOriginalRoles.length === 0) {
          this.errorMessage = 'The impersonated user has no roles and admin roles could not be loaded. Please contact your administrator.';
        }
        // If impersonating user has no roles but admin has roles, that's fine - admin roles will be shown
      },
      error: (err) => {
        console.error('Failed to get user info:', err);
        this.errorMessage = 'Failed to retrieve user information. You will be redirected to login.';
        this.isLoading = false;

        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 3000);
      }
    });
  }

  private mapRoleToInterface(roleName: string): UserRole | null {
    const roleMapping: { [key: string]: UserRole } = {
      'Admin': {
        name: 'Admin',
        displayName: 'Administrator',
        description: 'Full system access and user management capabilities',
        icon: 'M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.031 9-11.622 0-1.042-.133-2.052-.382-3.016z',
        route: '/admin',
        color: 'text-yellow-600',
        bgGradient: 'from-sky-400 to-sky-600'
      },
      'Manager': {
        name: 'Manager',
        displayName: 'Manager',
        description: 'Team management and leave request approvals',
        icon: 'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z',
        route: '/manager',
        color: 'text-sky-600',
        bgGradient: 'from-amber-300 to-amber-500'
      },
      'Employee': {
        name: 'Employee',
        displayName: 'Employee',
        description: 'Submit and manage your leave requests',
        icon: 'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z',
        route: '/user',
        color: 'text-green-600',
        bgGradient: 'from-green-400 to-green-600'
      },
      'HR': {
        name: 'HR',
        displayName: 'Human Resources',
        description: 'Manage employee records and leave requests',
        icon: 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z',
        route: '/hr',
        color: 'text-purple-600',
        bgGradient: 'from-purple-400 to-purple-600'
      }
    };

    return roleMapping[roleName] || null;
  }

  selectRole(role: UserRole) {
    console.log(`User selected role: ${role.name}, navigating to: ${role.route}`);
    this.router.navigate([role.route]);
  }

  selectAdminRole(role: UserRole) {
    console.log(`Admin selected original role while impersonating: ${role.name}`);
    
    // First stop impersonation, then navigate to admin role
    this.auth.stopImpersonation().subscribe({
      next: () => {
        this.auth.clearImpersonation();
        this.isImpersonating = false;
        this.impersonatedUserName = '';
        this.impersonatedRoles = [];
        this.adminOriginalRoles = [];
        
        // Navigate to the admin role
        console.log(`Impersonation stopped, navigating to admin role: ${role.route}`);
        this.router.navigate([role.route]);
      },
      error: (err) => {
        console.error('Error stopping impersonation for admin role switch:', err);
        // Still try to navigate and clear local state
        this.auth.clearImpersonation();
        this.isImpersonating = false;
        this.impersonatedUserName = '';
        this.impersonatedRoles = [];
        this.adminOriginalRoles = [];
        this.router.navigate([role.route]);
      }
    });
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }

  logout() {
    this.auth.logout().subscribe({
      next: () => {
        this.router.navigate(['/login']);
      },
      error: (err) => {
        console.error('Logout error:', err);
        this.router.navigate(['/login']);
      }
    });
  }

  stopImpersonation(): void {
    if (confirm('Are you sure you want to stop impersonating this user? You will return to your admin account.')) {
      this.auth.stopImpersonation().subscribe({
        next: () => {
          this.auth.clearImpersonation();
          this.isImpersonating = false;
          this.impersonatedUserName = '';
          this.impersonatedRoles = [];
          // Reload the user roles to get back the admin's roles
          this.loadUserRoles();
        },
        error: (err) => {
          console.error('Error stopping impersonation:', err);
          // Still clear local state even if backend call fails
          this.auth.clearImpersonation();
          this.isImpersonating = false;
          this.impersonatedUserName = '';
          this.impersonatedRoles = [];
          this.loadUserRoles();
        }
      });
    }
  }
}