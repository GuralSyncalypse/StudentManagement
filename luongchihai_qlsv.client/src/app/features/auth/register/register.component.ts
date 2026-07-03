import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  registerForm!: FormGroup;
  successMessage: string = '';
  errorMessage: string = '';
  isLoading: boolean = false;

  constructor(private authService: AuthService, private router: Router) { }

  ngOnInit(): void {
    this.registerForm = new FormGroup({
      username: new FormControl('', [Validators.required, Validators.minLength(4)]),
      password: new FormControl('', [Validators.required, Validators.minLength(4)]),
      email: new FormControl('', [Validators.required, Validators.email]),
      phoneNumber: new FormControl('', [Validators.required, Validators.pattern(/^\d{10}$/)]),
      roleName: new FormControl('Student', [Validators.required]),

      // Nhóm trường thông tin sinh viên
      studentID: new FormControl(''),
      studentName: new FormControl(''),
      gender: new FormControl('Nam')
    });

    // Lắng nghe thay đổi Role để bật/tắt validator cho thông tin sinh viên
    this.registerForm.get('roleName')?.valueChanges.subscribe(role => {
      this.toggleStudentValidators(role);
    });

    // Chạy kích hoạt lần đầu mặc định là Student
    this.toggleStudentValidators('Student');
  }

  toggleStudentValidators(role: string): void {
    const studentIDControl = this.registerForm.get('studentID');
    const studentNameControl = this.registerForm.get('studentName');

    if (role === 'Student') {
      studentIDControl?.setValidators([Validators.required]);
      studentNameControl?.setValidators([Validators.required]);
    } else {
      studentIDControl?.clearValidators();
      studentNameControl?.clearValidators();
    }
    studentIDControl?.updateValueAndValidity();
    studentNameControl?.updateValueAndValidity();
  }

  onSubmit(): void {
    console.log(this.registerForm)
    if (this.registerForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.register(this.registerForm.value).subscribe({
      next: (res) => {
        this.isLoading = false;
        this.successMessage = res.message || 'Đăng ký thành công! Đang chuyển hướng về trang đăng nhập...';
        setTimeout(() => this.router.navigate(['/login']), 2000);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Đăng ký thất bại. Vui lòng kiểm tra lại dữ liệu!';
      }
    });
  }
}
