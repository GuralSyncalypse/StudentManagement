import { Component, OnInit, inject, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StudentReportService } from '../../../core/services/report.services';
import { StudentSummary } from '../../../core/models/report.model';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './my-courses.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StudentCoursesComponent implements OnInit {
  private reportService = inject(StudentReportService);
  private cdr = inject(ChangeDetectorRef);

  summaryData?: StudentSummary;

  // Bộ lọc
  selectedSemester: number = 1;
  selectedYear: number = 2026;
  years: number[] = [2024, 2025, 2026, 2027];

  errorMessage?: string;

  get passedCount(): number {
    return this.summaryData?.coursesDetail.filter(c => c.result === 'Đạt').length ?? 0;
  }

  get failedCount(): number {
    return this.summaryData?.coursesDetail.filter(c => c.result !== 'Đạt').length ?? 0;
  }

  get totalCredits(): number {
    return this.summaryData?.coursesDetail.reduce((sum, c) => sum + c.credits, 0) ?? 0;
  }

  // ĐTB Thang 10
  get semesterGPA(): number {
    if (!this.summaryData?.coursesDetail || this.summaryData.coursesDetail.length === 0 || this.totalCredits === 0) {
      return 0;
    }
    const totalWeightedScore = this.summaryData.coursesDetail.reduce(
      (sum, c) => sum + (c.totalScore * c.credits),
      0
    );
    return totalWeightedScore / this.totalCredits;
  }

  // 🌟 Quy đổi ước tính GPA Thang 4
  // Helper chuyển Thang điểm chữ -> Điểm hệ 4 chuẩn tín chỉ
  private convertGradeToPoint4(grade: string): number {
    switch (grade?.trim().toUpperCase()) {
      case 'A+':
      case 'A': return 4.0;
      case 'B+': return 3.5;
      case 'B': return 3.0;
      case 'C+': return 2.5;
      case 'C': return 2.0;
      case 'D+': return 1.5;
      case 'D': return 1.0;
      default: return 0.0; // F hoặc chưa đạt
    }
  }

  // 🌟 Tính chính xác ĐTB Học kỳ Hệ 4 chuẩn Đại học
  get semesterGPA4(): number {
    if (!this.summaryData?.coursesDetail || this.summaryData.coursesDetail.length === 0 || this.totalCredits === 0) {
      return 0;
    }

    const totalWeightedPoint4 = this.summaryData.coursesDetail.reduce((sum, course) => {
      const point4 = this.convertGradeToPoint4(course.grade);
      return sum + (point4 * course.credits);
    }, 0);

    return totalWeightedPoint4 / this.totalCredits;
  }

  // 🌟 Helper class đổi màu theo Thang điểm chữ
  getGradeBadgeClass(grade: string): string {
    switch (grade?.toUpperCase()) {
      case 'A+':
      case 'A':
        return 'bg-emerald-100 text-emerald-800 border border-emerald-200';
      case 'B+':
      case 'B':
        return 'bg-blue-100 text-blue-800 border border-blue-200';
      case 'C+':
      case 'C':
        return 'bg-amber-100 text-amber-800 border border-amber-200';
      case 'D+':
      case 'D':
        return 'bg-orange-100 text-orange-800 border border-orange-200';
      default:
        return 'bg-rose-100 text-rose-800 border border-rose-200';
    }
  }

  ngOnInit(): void {
    this.loadData();
  }

  onFilterChange(): void {
    this.loadData();
  }

  // 🌟 Hàm in bảng điểm
  printReport(): void {
    window.print();
  }

  private loadData(): void {
    this.summaryData = undefined;
    this.errorMessage = undefined;
    this.cdr.markForCheck();

    this.reportService.getMySummary(this.selectedSemester, this.selectedYear).subscribe({
      next: (data: StudentSummary) => {
        this.summaryData = data;
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error(`Lỗi tải dữ liệu HK ${this.selectedSemester} - Năm ${this.selectedYear}:`, err);

        if (err.status === 404) {
          this.errorMessage = `Không tìm thấy kết quả học tập cho Học kỳ ${this.selectedSemester}, Năm ${this.selectedYear}.`;
        } else {
          this.errorMessage = 'Đã có lỗi hệ thống xảy ra. Vui lòng thử lại sau!';
        }

        this.cdr.markForCheck();
      }
    });
  }
}
