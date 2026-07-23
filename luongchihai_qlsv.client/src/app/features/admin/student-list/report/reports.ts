import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

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
  imports: [CommonModule, FormsModule],
  templateUrl: './reports.html'
})
export class StudentReportComponent {
  // Form controls
  studentId: string = '';
  semesterId: number = 1;
  year: number = new Date().getFullYear();

  // State Management
  summaryData: StudentSummary | null = null;
  isLoading: boolean = false;
  errorMessage: string = '';

  // Danh sách gợi ý chọn
  semesters = [
    { id: 1, label: 'Học kỳ 1' },
    { id: 2, label: 'Học kỳ 2' },
    { id: 3, label: 'Học kỳ Hè' }
  ];

  years: number[] = [2023, 2024, 2025, 2026];

  constructor(private http: HttpClient) { }

  fetchReport(): void {
    if (!this.studentId.trim()) {
      this.errorMessage = 'Vui lòng nhập Mã Sinh Viên!';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.summaryData = null;

    // Thay đổi URL API base phù hợp với môi trường dự án của bạn
    const url = `/api/Reports/semester-summary?studentId=${encodeURIComponent(this.studentId)}&semesterId=${this.semesterId}&year=${this.year}`;

    this.http.get<StudentSummary>(url).subscribe({
      next: (data) => {
        this.summaryData = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        if (err.status === 404) {
          this.errorMessage = err.error?.message || 'Không tìm thấy dữ liệu học tập của sinh viên này.';
        } else {
          this.errorMessage = 'Có lỗi xảy ra khi kết nối tới máy chủ!';
        }
      }
    });
  }

  // Hàm hỗ trợ format CSS class cho kết quả (Đạt/Trượt)
  getResultBadgeClass(result: string): string {
    const res = result?.toLowerCase() || '';
    if (res.includes('đạt') || res.includes('pass')) {
      return 'bg-emerald-100 text-emerald-800 border border-emerald-200';
    }
    if (res.includes('trượt') || res.includes('fail')) {
      return 'bg-rose-100 text-rose-800 border border-rose-200';
    }
    return 'bg-slate-100 text-slate-800 border border-slate-200';
  }
}
