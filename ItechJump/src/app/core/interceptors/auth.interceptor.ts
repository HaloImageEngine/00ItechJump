import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { SessionTokenService } from '../security/session-token.service';

/** Attaches Bearer session token from login when present. */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(SessionTokenService).get();
  if (!token) {
    return next(req);
  }
  return next(
    req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    })
  );
};