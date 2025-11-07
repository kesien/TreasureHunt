import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth';
import { map, take } from 'rxjs';
export const authRedirectGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  return authService.authState$.pipe(
    take(1),
    map((authState) => {
      if (authState.isAuthenticated) {
        console.log(authState.isAuthenticated);
        if (authState.userType === 'admin') {
          router.navigate(['/admin/dashboard']);
        } else if (authState.userType === 'team') {
          router.navigate(['/team/dashboard']);
        }
        return false;
      } else {
        return true;
      }
    })
  );
};
