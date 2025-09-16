import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { Auth } from '../services/authService/auth';
import { firstValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class LoginGuard implements CanActivate {
  constructor(private router: Router, private authService: Auth) {}

  async canActivate(): Promise<boolean | UrlTree> {
    const hasAuthCookies = this.hasAuthenticationCookies();

    if (!hasAuthCookies) {
      return true;
    }

    try {
      // Use the Auth service instead of direct fetch for consistency
      const data = await firstValueFrom(this.authService.me());

      if (data && data.userId && data.email && data.roles) {
        return this.router.parseUrl('/role-selector');
      }

      // User is not authenticated, allow access to login page
      return true;
    } catch (error) {
      console.error('LoginGuard error:', error);
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
