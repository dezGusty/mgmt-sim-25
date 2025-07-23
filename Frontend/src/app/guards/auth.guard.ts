import { Injectable } from '@angular/core';
import {
  CanActivate,
  Router,
  UrlTree,
  ActivatedRouteSnapshot,
} from '@angular/router';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private router: Router) {}

  canActivate(
    route: ActivatedRouteSnapshot
  ):
    | boolean
    | UrlTree
    | Promise<boolean | UrlTree>
    | Observable<boolean | UrlTree> {
    return fetch('https://localhost:7275/api/Auth/me', {
      credentials: 'include',
    })
      .then(async (response) => {
        if (!response.ok) return this.router.parseUrl('/login');
        const data = await response.json();
        if (data && data.userId && data.email && data.roles) {
          const allowedRoles = route.data['roles'] as string[] | undefined;
          if (!allowedRoles || allowedRoles.length === 0) {
            // No role restriction, just needs to be authenticated
            return true;
          }
          // Check if user has at least one allowed role
          if (data.roles.some((role: string) => allowedRoles.includes(role))) {
            return true;
          }
        }
        return this.router.parseUrl('/login');
      })
      .catch(() => this.router.parseUrl('/login'));
  }
}
