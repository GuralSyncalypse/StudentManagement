import { Component, inject } from '@angular/core';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-admin-layout', // Đổi selector cho đúng nghĩa bộ khung admin
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive], // Import các directive dùng trong HTML layout
  templateUrl: './admin-layout.component.html', // Trỏ về file HTML bộ khung (có Sidebar/Topbar)
  styleUrls: ['./admin-layout.component.css']
})
export class AdminLayoutComponent {
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
