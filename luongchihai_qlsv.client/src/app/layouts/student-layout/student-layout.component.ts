import { Component, inject } from '@angular/core';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-student-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive], // Import các directive dùng trong HTML layout
  templateUrl: './student-layout.component.html',
  styleUrl: './student-layout.component.css',
})
export class StudentLayoutComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  // Hàm xử lý khi bấm nút Đăng xuất
  onLogout(): void {
    if (confirm('Bạn có chắc chắn muốn đăng xuất không?')) {
      this.authService.logout();        // Xóa token trong localStorage/cookie
      this.router.navigate(['/login']); // Đá về trang login công khai
    }
  }
}
