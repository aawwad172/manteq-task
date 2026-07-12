import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { TokenService } from '../auth/token.service';

/**
 * Gates a route by permission claim. Attach the required permission via route data:
 *   { canActivate: [permissionGuard], data: { permission: Permissions.Requests.Approve } }
 * Falls back to the requests list if the user lacks the permission.
 */
export const permissionGuard: CanActivateFn = (route) => {
  const tokens = inject(TokenService);
  const router = inject(Router);

  const required = route.data['permission'] as string | undefined;
  if (!required || tokens.hasPermission(required)) {
    return true;
  }

  return router.createUrlTree(['/requests']);
};
