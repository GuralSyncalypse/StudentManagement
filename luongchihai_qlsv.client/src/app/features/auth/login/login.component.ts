import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html'
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  errorMessage: string = '';
  isLoading: boolean = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    // Khởi tạo Form và các điều kiện Validate dữ liệu
    this.loginForm = new FormGroup({
      username: new FormControl('', [Validators.required]),
      password: new FormControl('', [Validators.required, Validators.minLength(4)])
    });

    // Nếu đã đăng nhập trước đó, tự động chuyển vào Dashboard luôn
    if (this.authService.isLoggedIn()) {
      this.redirectByRole();
    }
  }

  onSubmit(): void {
    if (this.loginForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login(this.loginForm.value).subscribe({
      next: () => {
        this.isLoading = false;
        this.redirectByRole();
      },
      error: (err) => {
        this.isLoading = false;
        // 🔥 Đổi thành '.detail' để khớp với chuẩn Problem Details của .NET 8
        this.errorMessage = err.error?.detail || 'Đăng nhập thất bại. Vui lòng kiểm tra lại tài khoản!';
      }
    });
  }

  // Hàm điều hướng dựa trên quyền (Role-based Redirection)
  private redirectByRole(): void {
    const role = this.authService.getUserRole();
    console.log(role);
    if (role === 'Admin') {
      this.router.navigate(['/admin/dashboard']); // Admin
    } else if (role === 'Student') {
      this.router.navigate(['/student/dashboard']); // Sinh viên
    } else {
      this.router.navigate(['/dashboard']); // Trang chung
    }
  }
}
