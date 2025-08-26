import { Injectable } from '@angular/core';
import {
  CanActivate,
  Router,
  UrlTree,
  ActivatedRouteSnapshot,
} from '@angular/router';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private router: Router) { }

  async canActivate(route: ActivatedRouteSnapshot): Promise<boolean | UrlTree> {
    try {
      const response = await fetch(`${environment.apiUrl}/Auth/me`, {
        credentials: 'include',
      });

      if (!response.ok) {
        return this.router.parseUrl('/login');
      }

      const data = await response.json();

      if (data && data.userId && data.email && data.roles) {
        const effectiveRoles: string[] = data.roles;
        const isActingAsSecondManager: boolean = data.isActingAsSecondManager || false;
        const isTemporarilyReplaced: boolean = data.isTemporarilyReplaced || false;

        const allowedRoles = route.data['roles'] as string[] | undefined;

        if (!allowedRoles || allowedRoles.length === 0) {
          if (route.routeConfig?.path === 'login') {
            if (effectiveRoles.includes('Admin')) return this.router.parseUrl('/admin');
            if (effectiveRoles.includes('Manager'))
              return this.router.parseUrl('/manager');
            if (effectiveRoles.includes('Employee'))
              return this.router.parseUrl('/user');
            if (effectiveRoles.includes('HR'))
              return this.router.parseUrl('/hr');
          }
          return true;
        }

        if (effectiveRoles.some((role) => allowedRoles.includes(role))) {
          return true;
        }
      }

      return this.router.parseUrl('/role-selector');
    } catch {
      return this.router.parseUrl('/login');
    }
  }
}
