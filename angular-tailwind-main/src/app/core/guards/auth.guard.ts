import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { map, catchError, of } from 'rxjs';
import { AuthService } from "../services/auth.service";

export const authGuard = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isAuthenticated$.pipe(
    map(isAuthenticated => {
      if (isAuthenticated) {
        return true; // Allow access to protected routes
      }
      router.navigate(['/auth/sign-in']);
      return false;
    }),
    catchError(() => {
      router.navigate(['/auth/sign-in']);
      return of(false);
    })
  );
}; 