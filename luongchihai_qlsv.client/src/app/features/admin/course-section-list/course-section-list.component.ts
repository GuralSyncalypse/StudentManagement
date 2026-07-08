import { Component, OnInit, inject, ChangeDetectorRef, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop'; // Hoặc @angular/core/rxjs-interop tùy phiên bản Angular 16/17+
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CourseSectionService } from './course-section-list.services';
import { EnrollmentService } from '../../../core/services/enrollment.services';
import { CourseSection } from '../../../core/models/course.model';

@Component({
  selector: 'app-course-section-list',
  standalone: true,
  imports: [CommonModule, FormsModule], // ĐÃ THÊM: FormsModule
  templateUrl: './course-section-list.component.html',
  styleUrl: './course-section-list.component.css'
})
export class CourseSectionListComponent implements OnInit {
  private sectionService = inject(CourseSectionService);
  private enrollmentService = inject(EnrollmentService);
  private cdr = inject(ChangeDetectorRef);
  private destroyRef = inject(DestroyRef);

  // --- DỮ LIỆU BẢNG ---
  allSections: CourseSection[] = [];
  sections: CourseSection[] = [];
  isLoading: boolean = true;
  errorMessage?: string;

  // --- ĐÃ BỔ SUNG: BIẾN TRẠNG THÁI MODAL ĐĂNG KÝ (ADMIN) ---
  isRegModalOpen: boolean = false;
  selectedSection: CourseSection | null = null;
  studentIdInput: string = '';

  // --- ĐÃ BỔ SUNG: BIẾN PHỤC VỤ BỘ LỌC SONG SONG ---
  searchTerm: string = '';
  selectedStatus: string = 'All';

  ngOnInit(): void {
    this.loadOpenSections();
  }

  // Tải danh sách lớp học phần
  loadOpenSections(): void {
    this.isLoading = true;
    this.errorMessage = undefined;
    this.cdr.markForCheck();

    this.sectionService.getCourseSections()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (data) => {
          this.allSections = data;
          this.sections = data;
          this.isLoading = false;
          this.applyFilters();
        },
        error: (err) => {
          console.error('Lỗi lấy danh sách lớp học phần:', err);
          this.errorMessage = 'Không thể tải danh sách lớp học phần đang mở lúc này.';
          this.isLoading = false;
          this.cdr.markForCheck();
        }
      });
  }

  // Xử lý khi gõ ô Tìm kiếm
  onSearch(event: Event): void {
    this.searchTerm = (event.target as HTMLInputElement).value.toLowerCase().trim();
    this.applyFilters();
  }

  // ĐÃ BỔ SUNG: Xử lý khi chọn Dropdown trạng thái
  onFilterStatus(event: Event): void {
    this.selectedStatus = (event.target as HTMLSelectElement).value;
    this.applyFilters();
  }

  // ĐÃ CẢI TIẾN: Hàm lọc tổng hợp
  private applyFilters(): void {
    this.sections = this.allSections.filter(section => {
      // 1. Kiểm tra điều kiện tìm kiếm Text
      const matchSearch = !this.searchTerm || (
        section.courseID?.toLowerCase().includes(this.searchTerm) ||
        section.course?.courseName?.toLowerCase().includes(this.searchTerm) ||
        section.classSection?.toLowerCase().includes(this.searchTerm)
      );

      // 2. Kiểm tra điều kiện Dropdown Trạng thái
      const matchStatus = this.selectedStatus === 'All' || section.status === this.selectedStatus;

      return matchSearch && matchStatus;
    });

    this.cdr.markForCheck();
  }

  // 1. Khai báo thêm các State điều khiển Modal hủy đăng ký
  isUnregModalOpen: boolean = false;
  studentIdUnregInput: string = '';

  // 2. Hàm mở Modal Hủy Đăng ký
  onUnregister(section: any) {
    this.selectedSection = section;
    this.studentIdUnregInput = ''; // Reset input cũ
    this.isUnregModalOpen = true;
    this.cdr.markForCheck();
  }

  // 3. Hàm đóng Modal Hủy Đăng ký
  closeUnregModal() {
    this.isUnregModalOpen = false;
    this.selectedSection = null;
    this.studentIdUnregInput = '';
    this.cdr.markForCheck();
  }

  // Mở modal đăng ký hộ
  onRegister(section: CourseSection): void {
    this.selectedSection = section;
    this.studentIdInput = '';
    this.isRegModalOpen = true;
    this.cdr.markForCheck();
  }

  // Đóng modal đăng ký
  closeRegModal(): void {
    this.isRegModalOpen = false;
    this.selectedSection = null;
    this.studentIdInput = '';
    this.cdr.markForCheck();
  }

  submitAdminUnregistration() {
    const mssv = this.studentIdUnregInput.trim();
    if (!this.studentIdUnregInput.trim() || !this.selectedSection) return;

    const payload = {
      sectionID: this.selectedSection.sectionID,
      studentID: mssv
    };

    // Gọi Service API xử lý xóa (ví dụ mẫu)
    this.enrollmentService.adminCancelEnrollment(payload).subscribe({
      next: (res) => {
        alert(`🎉 Đã đăng ký thành công sinh viên [${mssv}] vào lớp!`);
        this.closeUnregModal();
        this.loadOpenSections();
      },
      error: (err) => {
        console.error('Lỗi xếp lớp:', err);
        alert(`⚠️ Thất bại: ${err.error?.message || 'Mã SV không tồn tại hoặc trùng lịch học!'}`);
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  // Xác nhận xếp lớp gửi lên Server
  submitAdminRegistration(): void {
    const mssv = this.studentIdInput.trim();

    if (!this.selectedSection || !mssv) {
      alert('Vui lòng nhập đầy đủ mã sinh viên!');
      return;
    }

    this.isLoading = true;
    this.cdr.markForCheck();

    const payload = {
      sectionID: this.selectedSection.sectionID,
      studentID: mssv
    };

    this.enrollmentService.adminRegisterEnrollment(payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          alert(`🎉 Huỷ đăng ký thành công cho sinh viên [${mssv}]!`);
          this.closeRegModal();
          this.loadOpenSections();
        },
        error: (err) => {
          console.error('Lỗi huỷ đăng ký:', err);
          alert(`⚠️ Thất bại: ${err.error?.message || 'Mã SV không tồn tại hoặc không có trong lớp học!'}`);
          this.isLoading = false;
          this.cdr.markForCheck();
        }
      });
  }
}
