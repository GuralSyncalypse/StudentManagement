import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  // Chỉ cần dùng Router để chuyển hướng khi bị 401
  constructor(private router: Router) { }

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        const problemDetails = error.error;
        const errorMessage = problemDetails?.detail || 'Đã xảy ra lỗi hệ thống!';

        switch (error.status) {
          case 401:
            // 🔥 KIỂM TRA: Có phải là lỗi 401 từ API Login không?
            const isLoginRequest = request.url.includes('/api/auth/login');

            if (isLoginRequest) {
              // Nếu đăng nhập sai, chỉ hiển thị lỗi nhập sai mật khẩu/tài khoản do backend trả về
              alert(errorMessage || 'Tài khoản hoặc mật khẩu không chính xác!');
            } else {
              // Nếu là các API khác (hết hạn Access Token thật và không refresh được nữa)
              alert('Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.');
              localStorage.removeItem('token');
              this.router.navigate(['/login']);
            }
            break;

          case 403:
            alert(errorMessage || 'Bạn không có quyền truy cập tính năng này!');
            break;

          case 404:
            alert('Không tìm thấy: ' + errorMessage);
            break;

          case 400:
            alert('Yêu cầu không hợp lệ: ' + errorMessage);
            break;

          case 500:
            alert('Lỗi hệ thống: Hệ thống đang bảo trì. Vui lòng quay lại sau.');
            break;

          default:
            alert('Lỗi không xác định: ' + errorMessage);
            break;
        }

        return throwError(() => error);
      })
    );
  }
}
