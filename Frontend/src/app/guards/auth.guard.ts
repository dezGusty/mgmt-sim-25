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
  constructor(private router: Router) {}

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
        const roles: string[] = data.roles;

        const allowedRoles = route.data['roles'] as string[] | undefined;

        // Dacă nu există restricție de rol (ex: la pagina principală sau login), dar utilizatorul e deja logat:
        if (!allowedRoles || allowedRoles.length === 0) {
          // Dacă e deja logat și încearcă să acceseze /login → redirect către pagina potrivită
          if (route.routeConfig?.path === 'login') {
            if (roles.includes('Admin')) return this.router.parseUrl('/admin');
            if (roles.includes('Manager'))
              return this.router.parseUrl('/manager');
            if (roles.includes('Employee'))
              return this.router.parseUrl('/user');
            if (roles.includes('HR'))
              return this.router.parseUrl('/hr');
          }
          return true;
        }

        // Verifică dacă are rolul necesar
        if (roles.some((role) => allowedRoles.includes(role))) {
          return true;
        }
      }

      return this.router.parseUrl('/role-selector');
    } catch {
      return this.router.parseUrl('/login');
    }
  }
}
