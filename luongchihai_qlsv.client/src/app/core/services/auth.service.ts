import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = '/api/auth'; // Sửa lại đúng Port Backend của bạn
  private authStatus = new BehaviorSubject<boolean>(this.isLoggedIn());

  constructor(private http: HttpClient) { }

  login(credentials: { username: string; passwordHash: string }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, credentials).pipe(
      tap(res => {
        if (res && res.token) {
          localStorage.setItem('token', res.token);
          this.authStatus.next(true);
        }
      })
    );
  }

  register(userData: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/register`, userData);
  }

  // Đăng xuất bằng cách xoá token khỏi localStorage
  logout(): void {
    localStorage.removeItem('token');
    this.authStatus.next(false);
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  // Giải mã payload JWT thủ công không cần cài thư viện ngoài
  getUserRole(): string {
    const token = localStorage.getItem('token');
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
