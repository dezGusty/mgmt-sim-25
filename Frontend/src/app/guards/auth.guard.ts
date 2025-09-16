import { Injectable } from '@angular/core';
import {
  CanActivate,
  Router,
  UrlTree,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
} from '@angular/router';
import { Observable } from 'rxjs';
import { Auth } from '../services/authService/auth';
import { firstValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private router: Router, private authService: Auth) { }

  async canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean | UrlTree> {
    try {
      // Use the Auth service instead of direct fetch to ensure consistency
      const data = await firstValueFrom(this.authService.me());

      if (!data) {
        return this.router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
      }

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
    } catch (error) {
      console.error('AuthGuard error:', error);
      return this.router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
    }
  }
}
