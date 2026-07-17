import { Component, OnInit, inject, ChangeDetectorRef, DestroyRef, ChangeDetectionStrategy } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CourseSectionService } from './course-section-list.services';
import { EnrollmentService } from '../../../core/services/enrollment.services';
import { AvailableCourseSection } from '../../../core/models/course.model';

interface SemesterFilterOption {
  semesterID: number;
  semesterDisplayName: string;
  academicYear: string;
}

interface GroupedCourse {
  courseID: string;
  courseName: string;
  semesterDisplayName: string;
  academicYear: string;
  isExpanded: boolean;
  sections: AvailableCourseSection[];
}

@Component({
  selector: 'app-course-section-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './course-section-list.component.html',
  styleUrl: './course-section-list.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CourseSectionListComponent implements OnInit {
  private sectionService = inject(CourseSectionService);
  private enrollmentService = inject(EnrollmentService);
  private cdr = inject(ChangeDetectorRef);
  private destroyRef = inject(DestroyRef);

  // --- DỮ LIỆU BẢNG ---
  allSections: AvailableCourseSection[] = [];
  groupedCourses: GroupedCourse[] = [];
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
  selectedSemesterId: string = 'All';
  uniqueSemesters: SemesterFilterOption[] = [];

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

  onFilterSemester(event: Event): void {
    this.selectedSemesterId = (event.target as HTMLSelectElement).value;
    this.applyFilters();
  }

  toggleCourseExpand(course: GroupedCourse): void {
    course.isExpanded = !course.isExpanded;
    this.cdr.markForCheck();
  }

  private applyFilters(): void {
    const filteredFlat = this.allSections.filter(section => {
      const matchSearch = !this.searchTerm || (
        section.courseID?.toLowerCase().includes(this.searchTerm) ||
        section.courseName?.toLowerCase().includes(this.searchTerm) ||
        section.classSection?.toLowerCase().includes(this.searchTerm)
      );
      const matchStatus = this.selectedStatus === 'All' || section.status === this.selectedStatus;
      const matchSemester = this.selectedSemesterId === 'All' || section.semesterID === +this.selectedSemesterId;

      return matchSearch && matchStatus && matchSemester;
    });

    const groups: { [key: string]: GroupedCourse } = {};

    filteredFlat.forEach(section => {
      const key = `${section.courseID}_${section.semesterID}`;
      if (!groups[key]) {
        groups[key] = {
          courseID: section.courseID || '',
          courseName: section.courseName || 'Chưa cập nhật tên môn',
          semesterDisplayName: section.semesterDisplayName || '',
          academicYear: section.academicYear || '',
          isExpanded: true,
          sections: []
        };
      }
      groups[key].sections.push(section);
    });

    this.groupedCourses = Object.values(groups);
    this.cdr.markForCheck();
  }

  // --- LOGIC ĐỔI MÀU SĨ SỐ (CHUYỂN TỪ HTML SANG) ---
  getSectionCapacityClass(section: AvailableCourseSection | null | undefined): string {
    if (!section || !section.maxCapacity) {
      return 'text-gray-700';
    }

    const current = section.currentEnrollment ?? 0;
    const max = section.maxCapacity;

    if (current >= max) {
      return 'text-red-600';
    }

    if (current / max >= 0.85) {
      return 'text-amber-600';
    }

    return 'text-gray-700';
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

  toggleAllCourses(expand: boolean): void {
    this.groupedCourses.forEach(c => c.isExpanded = expand);
    this.cdr.markForCheck();
  }

  onImportExcelClick(): void {
    alert('Tính năng nhập sinh viên hàng loạt bằng Excel đang được phát triển!');
  }

  onBulkUpdateStatus(course: any, status: 'Open' | 'Closed'): void {
    alert(`Đang cập nhật trạng thái các lớp của môn ${course.courseName} thành: ${status}`);
  }

  onToggleQuickStatus(section: any): void {
    section.status = section.status === 'Open' ? 'Closed' : 'Open';
    this.cdr.markForCheck();
  }

  onViewStudentsInSection(section: any): void {
    alert(`Đang mở danh sách sinh viên thực tế lớp ${section.classSection}`);
  }

  submitAdminRegistration(): void {
    const mssv = this.studentIdInput.trim();
    if (!this.selectedSection || !mssv) {
      alert('Vui lòng nhập đầy đủ mã sinh viên!');
      return;
    }

    this.isLoading = true;
    this.cdr.markForCheck();

    const payload = { sectionID: this.selectedSection.sectionID, studentID: mssv };

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

    this.isLoading = true;
    this.cdr.markForCheck();

    const payload = { sectionID: this.selectedSection.sectionID, studentID: mssv };

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
