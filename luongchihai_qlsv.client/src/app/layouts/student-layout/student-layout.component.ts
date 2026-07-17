import { Component, inject } from '@angular/core';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-student-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './student-layout.component.html',
})
export class StudentLayoutComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  // Hàm xử lý khi bấm nút Đăng xuất
  onLogout(): void {
    if (confirm('Bạn có chắc chắn muốn đăng xuất không?')) {
      this.authService.logout();
      this.router.navigate(['/login']); // Đá về trang login công khai
    }
  }
}
