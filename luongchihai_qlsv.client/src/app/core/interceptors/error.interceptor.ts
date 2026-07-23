import { Injectable, inject } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../services/auth.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  private toastr = inject(ToastrService);
  private authService = inject(AuthService);

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        const errorMessage = this.extractErrorMessage(error);

        switch (error.status) {
          case 0:
            this.toastr.error(
              'Không thể kết nối đến máy chủ. Vui lòng kiểm tra lại kết nối mạng!',
              'Lỗi kết nối'
            );
            break;

          case 401:
            // Kiểm tra request từ API Đăng nhập
            const isLoginRequest = request.url.includes('/api/auth/login');

            if (isLoginRequest) {
              this.toastr.error(
                errorMessage || 'Tài khoản hoặc mật khẩu không chính xác!',
                'Đăng nhập thất bại'
              );
            } else {
              this.toastr.warning(
                'Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.',
                'Hết hạn phiên'
              );
              this.authService.logout(); // Tự động xoá token và điều hướng về trang /login
            }
            break;

          case 403:
            this.toastr.error(
              errorMessage || 'Bạn không có quyền truy cập tính năng này!',
              'Từ chối truy cập'
            );
            break;

          case 404:
            this.toastr.error(`Không tìm thấy dữ liệu: ${errorMessage}`, 'Lỗi 404');
            break;

          case 400:
            this.toastr.error(`Yêu cầu không hợp lệ: ${errorMessage}`, 'Dữ liệu không hợp lệ');
            break;

          case 500:
            this.toastr.error(
              'Hệ thống đang bảo trì. Vui lòng quay lại sau.',
              'Lỗi hệ thống (500)'
            );
            break;

          default:
            this.toastr.error(
              `Lỗi không xác định (${error.status}): ${errorMessage}`,
              'Có lỗi xảy ra'
            );
            break;
        }

        return throwError(() => error);
      })
    );
  }

  /**
   * Bóc tách chi tiết chuỗi thông báo lỗi từ RFC 7807 ProblemDetails hoặc Backend Response
   */
  private extractErrorMessage(error: HttpErrorResponse): string {
    const problemDetails = error.error;
    if (!problemDetails) return 'Đã xảy ra lỗi không xác định!';

    // 1. Nếu backend trả về chuỗi trực tiếp
    if (typeof problemDetails === 'string') return problemDetails;

    // 2. Chuẩn ProblemDetails đơn giản (dùng trường detail)
    if (problemDetails.detail) return problemDetails.detail;

    // 3. Chuẩn ProblemDetails Validation Errors (ví dụ: ASP.NET Core ModelState)
    if (problemDetails.errors && typeof problemDetails.errors === 'object') {
      const firstKey = Object.keys(problemDetails.errors)[0];
      if (firstKey && problemDetails.errors[firstKey]?.length) {
        return problemDetails.errors[firstKey][0];
      }
    }

    return problemDetails.message || 'Đã xảy ra lỗi!';
  }
}
