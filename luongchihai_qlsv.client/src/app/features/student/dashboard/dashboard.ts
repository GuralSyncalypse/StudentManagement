import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { DecimalPipe, JsonPipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { DashboardData } from './dashboard.models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [DecimalPipe, JsonPipe],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class StudentDashboardComponent implements OnInit {
  // Thay inject qua constructor bằng inject() function chuẩn Angular v21
  private http = inject(HttpClient);

  // Khai báo các Signal quản lý State
  isLoading = signal<boolean>(true);
  error = signal<string | null>(null);
  data = signal<DashboardData | null>(null);

  // Computed Signal tự động tính toán class cho trạng thái học vụ
  statusClass = computed(() => {
    const status = this.data()?.profile.academicStatus;
    switch (status) {
      case 'Đang học': return 'px-3 py-1 bg-emerald-50 text-emerald-700 text-xs font-semibold rounded-full border border-emerald-100';
      case 'Bảo lưu': return 'px-3 py-1 bg-amber-50 text-amber-700 text-xs font-semibold rounded-full border border-amber-100';
      default: return 'px-3 py-1 bg-rose-50 text-rose-700 text-xs font-semibold rounded-full border border-rose-100';
    }
  });

  ngOnInit(): void {
    this.fetchDashboardData();
  }

  private fetchDashboardData(): void {
    // Gọi API từ backend (Đã cấu hình gộp dữ liệu từ 4 View)
    this.http.get<DashboardData>('/api/dashboards/student')
      .subscribe({
        next: (res) => {
          this.data.set(res);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error(err);
          this.error.set('Không thể tải dữ liệu bảng điểm. Vui lòng kiểm tra lại kết nối mạng!');
          this.isLoading.set(false);
        }
      });
  }

  // Hàm helper định hình màu sắc cho điểm chữ (Grade Letter)
  getGradeBadgeClass(grade: string): string {
    const base = 'px-2 py-0.5 rounded text-xs font-bold ';
    if (grade === 'A') return base + 'bg-emerald-100 text-emerald-800';
    if (grade === 'B') return base + 'bg-blue-100 text-blue-800';
    if (grade === 'C') return base + 'bg-amber-100 text-amber-800';
    if (grade === 'D') return base + 'bg-orange-100 text-orange-800';
    if (grade === 'F') return base + 'bg-rose-100 text-rose-800';
    return base + 'bg-slate-100 text-slate-500';
  }
}
