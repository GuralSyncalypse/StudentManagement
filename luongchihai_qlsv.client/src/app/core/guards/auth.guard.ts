import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Đọc mảng các Role được phép cấu hình ở file routing
  const expectedRoles = route.data['expectedRoles'] as Array<string>;
  const userRole = authService.getUserRole();

  if (authService.isLoggedIn() && expectedRoles.includes(userRole)) {
    return true;
  }

  // Nếu lậu hoặc sai quyền, đá văng về trang login
  alert('Bạn không có quyền truy cập khu vực này!');
  router.navigate(['/login']);
  return false;
};
