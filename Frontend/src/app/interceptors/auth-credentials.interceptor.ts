import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../environments/environment';

export const authCredentialsInterceptor: HttpInterceptorFn = (req, next) => {
  try {
    const apiBase = environment.apiUrl;
    if (typeof apiBase === 'string' && req.url.startsWith(apiBase)) {
      const reqWithCreds = req.clone({ withCredentials: true });
      return next(reqWithCreds);
    }
  } catch {}
  return next(req);
}; 