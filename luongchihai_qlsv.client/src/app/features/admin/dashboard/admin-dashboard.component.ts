import { Component, OnInit, inject, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { DecimalPipe } from '@angular/common';

interface DashboardSummary {
  totalStudents: number;
  totalCourses: number;
  totalSections: number;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [RouterLink, DecimalPipe],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css'],
  // Kích hoạt OnPush để tối ưu hiệu năng và kiểm soát UI chủ động qua CDR
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminDashboardComponent implements OnInit {
  private http = inject(HttpClient);
  private cdr = inject(ChangeDetectorRef); // Inject ChangeDetectorRef chuẩn Angular modern

  stats: DashboardSummary | null = null;
  isLoading = true;
  errorMessage = '';

  ngOnInit(): void {
    this.fetchDashboardData();
  }

  fetchDashboardData(): void {
    this.isLoading = true;
    this.errorMessage = '';
    // Thao tác đồng bộ, cần mark để UI hiển thị trạng thái loading ngay lập tức nếu cần
    this.cdr.markForCheck();

    this.http.get<DashboardSummary>('/api/Dashboards/admin').subscribe({
      next: (res) => {
        this.stats = res;
        this.isLoading = false;

        // Buộc Angular kiểm tra và render lại UI khi nhận được dữ liệu bất đồng bộ từ API
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Lỗi tải dữ liệu dashboard:', err);
        this.errorMessage = 'Không thể kết nối đến máy chủ để tải dữ liệu.';
        this.isLoading = false;

        this.stats = {
          totalStudents: 1248,
          totalCourses: 42,
          totalSections: 156
        };

        // Buộc Angular render lại kể cả khi lỗi để hiển thị đống Mock Data dự phòng bên trên
        this.cdr.markForCheck();
      }
    });
  }
}
