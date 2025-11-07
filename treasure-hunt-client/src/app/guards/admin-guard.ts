import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth';
import { map, take } from 'rxjs';
export const adminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  return authService.authState$.pipe(
    take(1),
    map((authState) => {
      if (authState.isAuthenticated) {
        if (authState.userType === 'admin') {
          return true;
        } else if (authState.userType === 'team') {
          router.navigate(['/team/dashboard']);
        }
        return true;
      } else {
        router.navigate(['/admin/login'])
        return false;
      }
    })
  );
};
