import { Component, signal, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { DecimalPipe } from '@angular/common';
import { finalize } from 'rxjs/operators';

export interface StudentCourseGrade {
  studentID: string;
  enrollmentID: number;
  sectionID: number;
  semesterID: number;
  semesterNo: number;
  startYear: number;
  academicYear: string;
  courseID: string;
  courseName: string;
  credits: number;
  totalScore: number;
  grade: string;
  result: string;
}

export interface StudentSummary {
  studentID: string;
  semesterID: number;
  semesterNo: number;
  startYear: number;
  totalEnrollment: number;
  coursesDetail: StudentCourseGrade[];
}

@Component({
  selector: 'app-student-report',
  standalone: true,
  imports: [FormsModule, DecimalPipe], // Không cần CommonModule nữa!
  templateUrl: './reports.html'
})
export class StudentReportComponent {
  private http = inject(HttpClient);

  // Form Controls
  studentId = signal<string>('2001234561');
  semesterId = signal<number>(1);
  year = signal<number>(2026);

  // State Management với Signals
  summaryData = signal<StudentSummary | null>(null);
  isLoading = signal<boolean>(false);
  errorMessage = signal<string>('');

  semesters = [
    { id: 1, label: 'Học kỳ 1' },
    { id: 2, label: 'Học kỳ 2' },
    { id: 3, label: 'Học kỳ Hè' }
  ];

  years: number[] = [2023, 2024, 2025, 2026];

  // Computed signals: Tự động tính toán lại khi summaryData thay đổi
  totalCredits = computed(() => {
    const details = this.summaryData()?.coursesDetail || [];
    return details.reduce((sum, item) => sum + Number(item.credits || 0), 0);
  });

  semesterGPA = computed(() => {
    const details = this.summaryData()?.coursesDetail || [];
    const credits = this.totalCredits();
    if (!details.length || credits === 0) return 0;

    const totalWeightedScore = details.reduce(
      (sum, item) => sum + (Number(item.totalScore || 0) * Number(item.credits || 0)),
      0
    );
    return totalWeightedScore / credits;
  });

  fetchReport(): void {
    const currentStudentId = this.studentId().trim();
    if (!currentStudentId) {
      this.errorMessage.set('Vui lòng nhập Mã Sinh Viên!');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');
    this.summaryData.set(null);

    const url = `/api/Reports/semester-summary?studentId=${encodeURIComponent(currentStudentId)}&semesterId=${this.semesterId()}&year=${this.year()}`;

    this.http.get<any>(url)
      .pipe(
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (data) => {
          if (data) {
            const courses = data.coursesDetail || data.CoursesDetail || [];
            this.summaryData.set({
              studentID: data.studentID || data.StudentID || currentStudentId,
              semesterID: data.semesterID || data.SemesterID || this.semesterId(),
              semesterNo: data.semesterNo || data.SemesterNo || this.semesterId(),
              startYear: data.startYear || data.StartYear || this.year(),
              totalEnrollment: data.totalEnrollment || data.TotalEnrollment || courses.length,
              coursesDetail: courses
            });
          }
        },
        error: (err) => {
          console.error('API Error:', err);
          this.errorMessage.set(err.error?.message || 'Có lỗi xảy ra khi tải dữ liệu!');
        }
      });
  }

  getResultBadgeClass(result: string): string {
    const res = (result || '').toLowerCase();
    if (res.includes('đạt') || res.includes('pass')) {
      return 'bg-emerald-50 text-emerald-700 border border-emerald-200';
    }
    if (res.includes('trượt') || res.includes('fail')) {
      return 'bg-rose-50 text-rose-700 border border-rose-200';
    }
    return 'bg-slate-100 text-slate-700 border border-slate-200';
  }

  // Bổ sung các phương thức này vào StudentReportComponent
  exportPrint(): void {
    window.print();
  }

  exportExcel(): void {
    // Logic xuất file Excel (sử dụng thư viện XLSX hoặc gọi API backend)
    console.log('Xuất file Excel cho sinh viên:', this.studentId());
  }

  viewCourseDetail(course: StudentCourseGrade): void {
    console.log('Xem chi tiết môn học:', course);
    // Bổ sung logic mở modal xem điểm thành phần tại đây
  }
}
