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
  userName = '';
  isLoading = true;
  errorMessage = '';

  // Carousel properties
  currentSlide = 0;
  slideWidth = 320;
  showCarouselControls = false;
  maxSlide = 0;
  slides: number[] = [];

  // Touch/swipe properties
  private touchStartX = 0;
  private touchEndX = 0;
  private isDragging = false;

  constructor(
    private router: Router,
    private auth: Auth
  ) { }

  ngOnInit() {
    this.loadUserRoles();
    this.setupKeyboardNavigation();
  }

  loadUserRoles() {
    this.isLoading = true;
    this.errorMessage = '';

    this.auth.me().subscribe({
      next: (user: any) => {
        this.auth.updateCurrentUser(user);

        this.userName = `${user.firstName || user.FirstName || ''} ${user.lastName || user.LastName || ''}`.trim()
          || user.email || user.Email || 'User';

        const userRoles = user.Roles || user.roles || [];

        this.userRoles = userRoles.map((role: string) => this.mapRoleToInterface(role))
          .filter((role: UserRole | null) => role !== null);

        this.isLoading = false;

        if (this.userRoles.length === 0) {
          this.errorMessage = 'No accessible roles found for your account. Please contact your administrator.';
        } else {
          this.initializeCarousel();
        }
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

  // Carousel initialization and control methods
  private initializeCarousel() {
    if (this.userRoles.length >= 3) {
      this.showCarouselControls = true;
      this.maxSlide = Math.max(0, this.userRoles.length - 1);
      this.slides = Array.from({ length: this.userRoles.length }, (_, i) => i);
      this.currentSlide = 0;
    } else {
      this.showCarouselControls = false;
    }
  }

  nextSlide() {
    if (this.currentSlide < this.maxSlide) {
      this.currentSlide++;
    }
  }

  previousSlide() {
    if (this.currentSlide > 0) {
      this.currentSlide--;
    }
  }

  goToSlide(slideIndex: number) {
    if (slideIndex >= 0 && slideIndex <= this.maxSlide) {
      this.currentSlide = slideIndex;
    }
  }

  onCardClick(role: UserRole, index: number) {
    if (index === this.currentSlide) {
      // If clicking the active card, select the role
      this.selectRole(role);
    } else {
      // If clicking a side card, navigate to it
      this.goToSlide(index);
    }
  }

  // Utility methods for carousel display
  getCardZIndex(index: number): number {
    if (index === this.currentSlide) {
      return 10; // Active card on top
    } else if (Math.abs(index - this.currentSlide) === 1) {
      return 5; // Adjacent cards
    } else {
      return 1; // Hidden cards
    }
  }

  isCardVisible(index: number): boolean {
    return Math.abs(index - this.currentSlide) <= 1;
  }

  // Touch/swipe event handlers
  onTouchStart(event: TouchEvent) {
    this.touchStartX = event.touches[0].clientX;
    this.isDragging = true;
  }

  onTouchMove(event: TouchEvent) {
    if (!this.isDragging) return;
    
    this.touchEndX = event.touches[0].clientX;
    // Prevent scrolling while swiping
    event.preventDefault();
  }

  onTouchEnd(event: TouchEvent) {
    if (!this.isDragging) return;
    
    this.isDragging = false;
    const swipeThreshold = 50; // Minimum swipe distance
    const swipeDistance = this.touchStartX - this.touchEndX;

    if (Math.abs(swipeDistance) > swipeThreshold) {
      if (swipeDistance > 0) {
        // Swiped left, go to next slide
        this.nextSlide();
      } else {
        // Swiped right, go to previous slide
        this.previousSlide();
      }
    }

    this.touchStartX = 0;
    this.touchEndX = 0;
  }

  // Keyboard navigation setup
  private setupKeyboardNavigation() {
    document.addEventListener('keydown', (event: KeyboardEvent) => {
      if (this.userRoles.length >= 3 && this.showCarouselControls) {
        switch (event.key) {
          case 'ArrowLeft':
            event.preventDefault();
            this.previousSlide();
            break;
          case 'ArrowRight':
            event.preventDefault();
            this.nextSlide();
            break;
          case 'Enter':
            event.preventDefault();
            if (this.userRoles[this.currentSlide]) {
              this.selectRole(this.userRoles[this.currentSlide]);
            }
            break;
        }
      }
    });
  }
}