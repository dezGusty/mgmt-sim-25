import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { Auth } from '../services/authService/auth';

export const authCredentialsInterceptor: HttpInterceptorFn = (req, next) => {
  try {
    const apiBase = environment.apiUrl;
    if (typeof apiBase === 'string' && req.url.startsWith(apiBase)) {
      const authService = inject(Auth);
      
      // Get authentication context for logging/debugging
      const authContext = authService.getAuthenticationContext();
      
      // Clone the request with credentials (cookies will automatically contain the appropriate token/session)
      const reqWithCreds = req.clone({ 
        withCredentials: true,
        setHeaders: {
          // Add a header for debugging purposes to track which authentication context is being used
          'X-Auth-Context': authContext.useImpersonationToken ? 'impersonation' : 'admin',
          'X-Effective-User-Id': authContext.effectiveUserId || 'unknown'
        }
      });
      
      // In development, log the authentication context being used
      if (!environment.production && authContext.isImpersonating) {
        console.log('HTTP Request - Authentication Context:', {
          url: req.url,
          isImpersonating: authContext.isImpersonating,
          effectiveUserId: authContext.effectiveUserId,
          useImpersonationToken: authContext.useImpersonationToken
        });
      }
      
      return next(reqWithCreds);
    }
  } catch (error) {
    // Log error in development
    if (!environment.production) {
      console.error('Auth interceptor error:', error);
    }
  }
  return next(req);
}; 