import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, catchError, Observable, tap, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = '/api/auth';

  // 1. Khởi tạo Signal bằng giá trị trong localStorage để khi F5 trang không bị mất trạng thái
  accessToken = signal<string | null>(localStorage.getItem('token'));

  // Đồng bộ trạng thái AuthStatus dựa trên sự tồn tại của Token
  private authStatus = new BehaviorSubject<boolean>(this.isLoggedIn());
  authStatus$ = this.authStatus.asObservable(); // Expose ra dạng Observable để các component subscribe

  constructor(private http: HttpClient) { }

  login(credentials: { username: string; passwordHash: string }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, credentials).pipe(
      tap(res => {
        if (res && res.token) {
          // ĐỒNG BỘ: Lưu vào cả LocalStorage và Signal (RAM)
          localStorage.setItem('token', res.token);
          this.accessToken.set(res.token);

          this.authStatus.next(true);
        }
      })
    );
  }

  // API âm thầm gia hạn Access Token mới
  refreshToken(): Observable<any> {
    console.log("Gọi API")
    return this.http.post<any>(`${this.apiUrl}/refresh-token`, {}, { withCredentials: true }).pipe(
      tap((res) => {
        if (res && res.token) {
          // ĐỒNG BỘ: Cập nhật token mới vào cả LocalStorage và Signal
          localStorage.setItem('token', res.token);
          this.accessToken.set(res.token);

          this.authStatus.next(true);
        }
      }),
      catchError((err) => {
        this.logout(); // Nếu lỗi (Refresh Token hết hạn), ép buộc đăng xuất
        return throwError(() => err);
      })
    );
  }

  register(userData: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/register`, userData);
  }

  // Đăng xuất và dọn dẹp sạch sẽ bộ nhớ
  logout(): void {
    // 1. Xoá token khỏi cả RAM (Signal) và LocalStorage ngay lập tức để tránh block UI
    this.accessToken.set(null);
    localStorage.removeItem('token');
    this.authStatus.next(false);

    // 2. Gọi API logout âm thầm để backend thu hồi token trong DB và xóa cookie ở trình duyệt
    this.http.post(`${this.apiUrl}/logout`, {}, { withCredentials: true }).subscribe({
      error: (err) => console.error('Lỗi gọi API Logout phía Backend:', err)
    });
  }

  isLoggedIn(): boolean {
    // Check đồng thời cả hai bộ nhớ cho chắc chắn
    return !!this.accessToken() || !!localStorage.getItem('token');
  }

  // Giải mã payload JWT thủ công không cần cài thư viện ngoài
  getUserRole(): string {
    // Ưu tiên đọc từ RAM (Signal) trước vì tốc độ phản hồi nhanh hơn và luôn là token mới nhất
    const token = this.accessToken() || localStorage.getItem('token');
    if (!token) return '';
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      // Claim Role mặc định của ASP.NET Core phát sinh khi đóng gói token
      return payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload['role'] || '';
    } catch (e) {
      return '';
    }
  }
}
