import { Component, inject, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { EnrollmentService } from './enrollment-list.service';
import { ScoreService } from '../../../core/services/score.service';
import { Enrollment, Score } from '../../../core/models/enrollment.model';

@Component({
  selector: 'app-enrollment-list',
  standalone: true,
  imports: [CommonModule, RouterModule, DatePipe, DecimalPipe, FormsModule],
  templateUrl: './enrollment-list.html',
  styleUrl: './enrollment-list.css',
})
export class EnrollmentList implements OnInit {
  private enrollmentService = inject(EnrollmentService);
  private scoreService = inject(ScoreService);
  private cdr = inject(ChangeDetectorRef);

  allEnrollments: Enrollment[] = [];
  enrollments: Enrollment[] = [];
  scoreTypes: string[] = [
    'Điểm chuyên cần',
    'Điểm bài tập / Thực hành',
    'Điểm kiểm tra giữa kỳ',
    'Điểm thi kết thúc học phần'
  ];
  selectedEnrollment: Enrollment | null = null;
  searchTerm = '';
  selectedScoreState = 'All';

  ngOnInit(): void {
    this.loadEnrollments();
  }

  // Chọn một dòng để hiển thị chi tiết nhập điểm
  selectEnrollment(enrollment: Enrollment): void {
    // Deep clone để tránh thay đổi trực tiếp data trên bảng chính khi chưa ấn Lưu
    this.selectedEnrollment = JSON.parse(JSON.stringify(enrollment));
  }

  // Tự động thêm một hàng điểm trống
  addNewScoreRow(): void {
    if (this.selectedEnrollment) {
      const newScore: Score = {
        scoreID: 0,
        enrollmentID: this.selectedEnrollment.enrollmentID,
        scoreType: '',
        weight: 0,
        scoreValue: 0
      };
      this.selectedEnrollment.scores.push(newScore);
    }
  }

  // Gửi dữ liệu điểm về cho Backend lưu (Đã dọn dẹp code thừa)
  saveScores(): void {
    if (!this.selectedEnrollment) return;

    // 1. Validate tổng trọng số phải bằng 100%
    const totalWeight = this.selectedEnrollment.scores.reduce((sum, s) => sum + (s.weight || 0), 0);
    if (totalWeight !== 100 && this.selectedEnrollment.scores.length > 0) {
      alert(`Cảnh báo: Tổng trọng số các đầu điểm hiện tại là ${totalWeight}%, vui lòng cấu hình đủ 100%!`);
      return;
    }

    // 2. Gọi API thật từ Server
    this.scoreService.saveScores(this.selectedEnrollment.scores)
      .subscribe({
        next: () => {
          alert('Cập nhật bảng điểm thành công!');
          this.loadEnrollments(); // Tải lại bảng chính để cập nhật điểm mới
          this.selectedEnrollment = null; // Đóng panel nhập điểm
        },
        error: (err) => {
          console.error('Lỗi khi lưu điểm:', err);
          alert('Không thể lưu điểm. Vui lòng kiểm tra lại cấu hình Backend!');
        }
      });
  }

  calculateTotalScore(): number {
    if (!this.selectedEnrollment || !this.selectedEnrollment.scores.length) {
      return 0;
    }

    let totalScore = 0;

    for (const score of this.selectedEnrollment.scores) {
      const value = score.scoreValue || 0;
      const weight = score.weight || 0;

      // Tính tổng theo công thức (Điểm * Trọng số / 100)
      totalScore += value * (weight / 100);
    }

    return totalScore;
  }

  // Tải danh sách đăng ký học phần
  loadEnrollments() {
    this.enrollmentService.getEnrollments()
      .subscribe({
        next: (res) => {
          this.allEnrollments = res;
          this.applyFilters();
        },
        error: (err) => {
          console.error('Lỗi khi tải danh sách đăng ký:', err);
        }
      });
  }

  onSearch(value: string): void {
    this.searchTerm = value.trim().toLowerCase();
    this.applyFilters();
  }

  onScoreStateChange(value: string): void {
    this.selectedScoreState = value;
    this.applyFilters();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedScoreState = 'All';
    this.applyFilters();
  }

  applyFilters(): void {
    const normalizedSearch = this.searchTerm;

    this.enrollments = this.allEnrollments.filter(enrollment => {
      const hasScores = (enrollment.scores?.length ?? 0) > 0;
      const matchSearch = !normalizedSearch || [
        String(enrollment.enrollmentID),
        enrollment.studentID,
        String(enrollment.sectionID)
      ].some(value => value?.toLowerCase().includes(normalizedSearch));

      const matchScoreState =
        this.selectedScoreState === 'All' ||
        (this.selectedScoreState === 'HasScores' && hasScores) ||
        (this.selectedScoreState === 'NoScores' && !hasScores);

      return matchSearch && matchScoreState;
    });

    this.cdr.detectChanges();
  }

  // Xóa lượt đăng ký
  deleteEnrollment(id: number) {
    const confirmDelete = confirm('Bạn có chắc chắn muốn xoá lượt đăng ký này không?');
    if (!confirmDelete) return;

    this.enrollmentService.deleteEnrollment(id)
      .subscribe({
        next: () => {
          alert('Xóa lượt đăng ký thành công!');
          if (this.selectedEnrollment?.enrollmentID === id) {
            this.selectedEnrollment = null;
          }
          this.loadEnrollments();
        },
        error: (err) => {
          console.error('Lỗi khi xóa đăng ký:', err);
        }
      });
  }
}
