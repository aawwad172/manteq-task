import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { environment } from '../../../environments/environment';
import { TokenService } from '../auth/token.service';

/**
 * Attaches the JWT as a Bearer Authorization header on requests to the backend API.
 * Only decorates calls to our own API base URL so the token never leaks to third parties.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(TokenService).token();

  if (token && req.url.startsWith(environment.apiBaseUrl)) {
    return next(
      req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }),
    );
  }

  return next(req);
};
