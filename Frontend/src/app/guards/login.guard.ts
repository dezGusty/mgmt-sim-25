import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class LoginGuard implements CanActivate {
  constructor(private router: Router) {}

  async canActivate(): Promise<boolean | UrlTree> {
    try {
      const response = await fetch('https://localhost:7275/api/Auth/me', {
        credentials: 'include',
      });

      if (response.ok) {
        const data = await response.json();

        if (data && data.userId && data.email && data.roles) {
          const roles: string[] = data.roles;

          // User is authenticated, redirect to appropriate dashboard
          if (roles.includes('Admin')) return this.router.parseUrl('/admin');
          if (roles.includes('Manager'))
            return this.router.parseUrl('/manager');
          if (roles.includes('Employee')) return this.router.parseUrl('/user');
          return this.router.parseUrl('/');
        }
      }

      // User is not authenticated, allow access to login page
      return true;
    } catch {
      // If there's an error, allow access to login page
      return true;
    }
  }
}
