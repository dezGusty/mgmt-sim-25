import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class LoginGuard implements CanActivate {
  constructor(private router: Router) {}

  async canActivate(): Promise<boolean | UrlTree> {
    const hasAuthCookies = this.hasAuthenticationCookies();

    if (!hasAuthCookies) {
      return true;
    }

    try {
      const response = await fetch(`${environment.apiUrl}/Auth/me`, {
        credentials: 'include',
      });

      if (response.ok) {
        const data = await response.json();

        if (data && data.userId && data.email && data.roles) {
          return this.router.parseUrl('/role-selector');
        }
      }

      // User is not authenticated, allow access to login page
      return true;
    } catch {
      // If there's an error, allow access to login page
      return true;
    }
  }

  private hasAuthenticationCookies(): boolean {
    const cookies = document.cookie;

    const authCookieNames = [
      '.AspNetCore.Identity.Application',
      'auth-token',
      'session-id',
      'access-token',
      'ManagementSimulator.Auth'
    ];

    return authCookieNames.some(cookieName =>
      cookies.includes(`${cookieName}=`)
    );
  }
}
