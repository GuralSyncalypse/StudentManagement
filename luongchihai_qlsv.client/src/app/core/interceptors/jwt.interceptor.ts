import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, take, switchMap } from 'rxjs/operators';
import { AuthService } from '../services/auth.service'; // Import AuthService của bạn

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  // Biến trạng thái để tránh gọi API Refresh Token trùng lặp
  private isRefreshing = false;
  // Hàng đợi lưu các request bị lỗi 401 trong lúc chờ Token mới
  private refreshTokenSubject: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);

  // Inject AuthService thông qua Constructor truyền thống
  constructor(private authService: AuthService) { }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // 1. Lấy Access Token (Ưu tiên lấy từ RAM/Signal trước, nếu không có thì fallback về localStorage)
    const token = this.authService.accessToken() || localStorage.getItem('token');

    let authReq = request;

    // 2. Nếu có token, tiến hành nhân bản request và đính kèm header Bearer
    if (token) {
      authReq = request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }

    // QUAN TRỌNG: Đính kèm withCredentials để trình duyệt tự động gửi kèm HttpOnly Cookie chứa Refresh Token
    authReq = authReq.clone({ withCredentials: true });

    // 3. Thực hiện request và bắt lỗi 401 (Hết hạn Access Token)
    return next.handle(authReq).pipe(
      catchError((error) => {
        if (error instanceof HttpErrorResponse && error.status === 401) {

          // 🔥 ĐIỀU KIỆN QUAN TRỌNG: 
          // Chỉ gọi Refresh Token nếu API bị lỗi KHÔNG PHẢI là API Login hoặc API Refresh-token
          const isLoginRequest = request.url.includes('/api/auth/login');
          const isRefreshRequest = request.url.includes('/api/auth/refresh-token');

          if (!isLoginRequest && !isRefreshRequest) {
            return this.handle401Error(authReq, next);
          }
        }

        // Trả lỗi về cho ErrorInterceptor hoặc Component tự xử lý
        return throwError(() => error);
      })
    );
  }

  // Hàm xử lý âm thầm làm mới Token và chạy lại các request lỗi
  private handle401Error(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null); // Reset trạng thái hàng đợi

      // Gọi API refresh token từ AuthService
      return this.authService.refreshToken().pipe(
        switchMap((res: any) => {
          this.isRefreshing = false;

          // Cập nhật lại token mới vào localStorage (nếu hệ thống gốc của bạn lưu ở đây)
          localStorage.setItem('token', res.token);

          // Phát tín hiệu cho các request khác đang xếp hàng đi tiếp
          this.refreshTokenSubject.next(res.token);

          // Chạy lại request bị lỗi ban đầu với token mới vừa nhận
          return next.handle(
            request.clone({
              setHeaders: {
                Authorization: `Bearer ${res.token}`
              }
            })
          );
        }),
        catchError((err) => {
          this.isRefreshing = false;
          this.authService.logout(); // Nếu refresh thất bại hoàn toàn, ép buộc logout
          return throwError(() => err);
        })
      );
    } else {
      // Nếu đã có một tiến trình refresh đang chạy, ép các request sau xếp hàng chờ
      return this.refreshTokenSubject.pipe(
        filter(token => token !== null), // Đợi cho đến khi token mới khác null
        take(1),                         // Lấy token mới nhất rồi đóng kết nối chờ
        switchMap((token) => {
          // Chạy lại request bị nghẽn với token mới
          return next.handle(
            request.clone({
              setHeaders: {
                Authorization: `Bearer ${token}`
              }
            })
          );
        })
      );
    }
  }
}
