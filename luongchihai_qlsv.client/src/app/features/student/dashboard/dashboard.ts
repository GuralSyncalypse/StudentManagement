import { Component, OnInit, inject, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StudentReportService } from '../../../core/services/report.services'; // Điều chỉnh lại đường dẫn thực tế của bạn
import { StudentSummary } from '../../../core/models/report.model'; // Nơi bạn lưu interface StudentSummary

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StudentDashboardComponent implements OnInit {
  private reportService = inject(StudentReportService);
  private cdr = inject(ChangeDetectorRef);

  summaryData?: StudentSummary;
  selectedSemester: number = 1;

  // 🌟 Thêm biến quản lý thông báo lỗi công khai
  errorMessage?: string;

  ngOnInit(): void {
    this.loadDataBySemester(this.selectedSemester);
  }

  onSemesterChange(event: Event): void {
    const selectElement = event.target as HTMLSelectElement;
    this.selectedSemester = Number(selectElement.value);
    this.loadDataBySemester(this.selectedSemester);
  }

  private loadDataBySemester(semester: number): void {
    // ⏳ Xóa dữ liệu cũ và lỗi cũ để chuẩn bị đón dữ liệu mới
    this.summaryData = undefined;
    this.errorMessage = undefined;
    this.cdr.markForCheck();

    this.reportService.getMySummary(semester).subscribe({
      next: (data: any) => {
        this.summaryData = data;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error(`Không thể tải dữ liệu của học kỳ ${semester}:`, err);

        // 🌟 Bắt lỗi dựa trên mã HTTP Status trả về từ API
        if (err.status === 404) {
          this.errorMessage = `Không tìm thấy dữ liệu kết quả học tập cho Học kỳ ${semester}.`;
        } else {
          this.errorMessage = 'Đã có lỗi hệ thống xảy ra. Vui lòng thử lại sau!';
        }

        // 🌟 Ép Angular cập nhật UI sang trạng thái báo lỗi (Do đang dùng OnPush)
        this.cdr.markForCheck();
      }
    });
  }
}
