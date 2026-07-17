import { Component, OnInit, inject, ChangeDetectorRef, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CourseSectionService } from './course-section-list.services';
import { EnrollmentService } from '../../../core/services/enrollment.services';
import { AvailableCourseSection } from '../../../core/models/course.model';

// Định nghĩa interface nội bộ cho danh sách bộ lọc học kỳ
interface SemesterFilterOption {
  semesterID: number;
  semesterDisplayName: string;
  academicYear: string;
}

@Component({
  selector: 'app-course-section-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './course-section-list.component.html',
  styleUrl: './course-section-list.component.css'
})
export class CourseSectionListComponent implements OnInit {
  private sectionService = inject(CourseSectionService);
  private enrollmentService = inject(EnrollmentService);
  private cdr = inject(ChangeDetectorRef);
  private destroyRef = inject(DestroyRef);

  // --- DỮ LIỆU BẢNG ---
  allSections: AvailableCourseSection[] = [];
  sections: AvailableCourseSection[] = [];
  isLoading: boolean = true;
  errorMessage?: string;

  // --- TRẠNG THÁI MODAL (ADMIN) ---
  isRegModalOpen: boolean = false;
  isUnregModalOpen: boolean = false;
  selectedSection: AvailableCourseSection | null = null;
  studentIdInput: string = '';
  studentIdUnregInput: string = '';

  // --- BỘ LỌC TÌM KIẾM ---
  searchTerm: string = '';
  selectedStatus: string = 'All';
  selectedSemesterId: string = 'All'; // THÊM MỚI: Trạng thái lọc học kỳ đang chọn
  uniqueSemesters: SemesterFilterOption[] = []; // THÊM MỚI: Danh sách danh mục học kỳ duy nhất

  ngOnInit(): void {
    this.loadOpenSections();
  }

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

          // THÊM MỚI: Trích xuất tự động danh sách Học kỳ duy nhất không trùng lặp từ Backend trả về
          this.extractUniqueSemesters(data);

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

  // THÊM MỚI: Hàm gom cụm danh sách Học kỳ để đưa vào Dropdown bộ lọc
  private extractUniqueSemesters(data: AvailableCourseSection[]): void {
    const seenIds = new Set<number>();
    this.uniqueSemesters = [];

    data.forEach(item => {
      if (item.semesterID && !seenIds.has(item.semesterID)) {
        seenIds.add(item.semesterID);
        this.uniqueSemesters.push({
          semesterID: item.semesterID,
          semesterDisplayName: item.semesterDisplayName || `Học kỳ ${item.semesterNo}`,
          academicYear: item.academicYear
        });
      }
    });

    // Sắp xếp học kỳ theo thứ tự thời gian hiển thị (nếu cần)
    this.uniqueSemesters.sort((a, b) => b.semesterID - a.semesterID);
  }

  onSearch(event: Event): void {
    this.searchTerm = (event.target as HTMLInputElement).value.toLowerCase().trim();
    this.applyFilters();
  }

  onFilterStatus(event: Event): void {
    this.selectedStatus = (event.target as HTMLSelectElement).value;
    this.applyFilters();
  }

  // THÊM MỚI: Xử lý sự kiện khi Admin thay đổi dropdown lọc học kỳ
  onFilterSemester(event: Event): void {
    this.selectedSemesterId = (event.target as HTMLSelectElement).value;
    this.applyFilters();
  }

  private applyFilters(): void {
    this.sections = this.allSections.filter(section => {
      // 1. Kiểm tra tìm kiếm text (Mã môn, Tên môn, Lớp bố trí)
      const matchSearch = !this.searchTerm || (
        section.courseID?.toLowerCase().includes(this.searchTerm) ||
        section.courseName?.toLowerCase().includes(this.searchTerm) ||
        section.classSection?.toLowerCase().includes(this.searchTerm)
      );

      // 2. Kiểm tra bộ lọc trạng thái (Open / Closed)
      const matchStatus = this.selectedStatus === 'All' || section.status === this.selectedStatus;

      // 3. THÊM MỚI: Kiểm tra bộ lọc học kỳ tuyến tính
      const matchSemester = this.selectedSemesterId === 'All' || section.semesterID === +this.selectedSemesterId;

      return matchSearch && matchStatus && matchSemester;
    });

    this.cdr.markForCheck();
  }

  onRegister(section: AvailableCourseSection): void {
    this.selectedSection = section;
    this.studentIdInput = '';
    this.isRegModalOpen = true;
    this.cdr.markForCheck();
  }

  closeRegModal(): void {
    this.isRegModalOpen = false;
    this.selectedSection = null;
    this.studentIdInput = '';
    this.cdr.markForCheck();
  }

  onUnregister(section: AvailableCourseSection): void {
    this.selectedSection = section;
    this.studentIdUnregInput = '';
    this.isUnregModalOpen = true;
    this.cdr.markForCheck();
  }

  closeUnregModal(): void {
    this.isUnregModalOpen = false;
    this.selectedSection = null;
    this.studentIdUnregInput = '';
    this.cdr.markForCheck();
  }

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
          alert(`🎉 Đăng ký thành công cho sinh viên [${mssv}] vào lớp!`);
          this.closeRegModal();
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

  submitAdminUnregistration(): void {
    const mssv = this.studentIdUnregInput.trim();
    if (!mssv || !this.selectedSection) return;

    const payload = {
      sectionID: this.selectedSection.sectionID,
      studentID: mssv
    };

    this.enrollmentService.adminCancelEnrollment(payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          alert(`🎉 Đã huỷ đăng ký thành công sinh viên [${mssv}] ra khỏi lớp!`);
          this.closeUnregModal();
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
