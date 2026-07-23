import { Component, OnInit, inject, ChangeDetectorRef, DestroyRef, ChangeDetectionStrategy } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CourseSectionService } from '../../../core/services/course-section.services';
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

export interface EnrolledStudent {
  studentID: string;
  fullName: string;
  email?: string;
  enrollmentDate?: string;
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

  // --- TRẠNG THÁI MODAL ĐĂNG KÝ HỘ (ADMIN) ---
  isRegModalOpen: boolean = false;
  selectedSection: AvailableCourseSection | null = null;
  studentIdInput: string = '';

  // --- BỘ LỌC TÌM KIẾM ---
  searchTerm: string = '';
  selectedStatus: string = 'All';
  selectedSemesterId: string = 'All';
  uniqueSemesters: SemesterFilterOption[] = [];

  // --- TRẠNG THÁI MODAL XEM SINH VIÊN ---
  isStudentModalOpen: boolean = false;
  isLoadingStudents: boolean = false;
  viewingSection: AvailableCourseSection | null = null;
  enrolledStudents: EnrolledStudent[] = [];

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

  /**
   * Mở modal và tải danh sách sinh viên của lớp học phần
   */
  onViewStudentsInSection(section: AvailableCourseSection): void {
    this.viewingSection = section;
    this.isStudentModalOpen = true;
    this.loadStudentsBySection(section.sectionID);
    this.cdr.markForCheck();
  }

  /**
   * Đóng modal danh sách sinh viên
   */
  closeStudentModal(): void {
    this.isStudentModalOpen = false;
    this.viewingSection = null;
    this.enrolledStudents = [];
    this.cdr.markForCheck();
  }

  /**
   * Gọi API tải danh sách sinh viên thuộc lớp
   */
  private loadStudentsBySection(sectionID: number): void {
    this.isLoadingStudents = true;
    this.cdr.markForCheck();

    this.enrollmentService.getStudentsBySection(sectionID)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (students: EnrolledStudent[]) => {
          this.enrolledStudents = students;
          this.isLoadingStudents = false;
          this.cdr.markForCheck();
        },
        error: (err) => {
          console.error('Lỗi lấy danh sách sinh viên:', err);
          this.enrolledStudents = [];
          this.isLoadingStudents = false;
          this.cdr.markForCheck();
        }
      });
  }

  /**
   * Xóa trực tiếp sinh viên khỏi lớp từ Modal
   */
  onRemoveStudentFromModal(student: EnrolledStudent): void {
    if (!this.viewingSection) return;

    const confirmDelete = confirm(`Bạn có chắc chắn muốn xóa sinh viên [${student.fullName} - ${student.studentID}] khỏi lớp ${this.viewingSection.classSection}?`);
    if (!confirmDelete) return;

    this.isLoading = true;
    this.cdr.markForCheck();

    const payload = {
      sectionID: this.viewingSection.sectionID,
      studentID: student.studentID
    };

    this.enrollmentService.adminCancelEnrollment(payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          alert(`🎉 Đã xóa sinh viên ${student.fullName} (${student.studentID}) khỏi lớp!`);
          this.enrolledStudents = this.enrolledStudents.filter(s => s.studentID !== student.studentID);
          this.isLoading = false;

          // Tải lại bảng chính để cập nhật sĩ số mới nhất
          this.loadOpenSections();
        },
        error: (err) => {
          console.error('Lỗi xóa sinh viên:', err);
          alert(`⚠️ Thất bại: ${err.error?.message || 'Không thể xóa sinh viên lúc này!'}`);
          this.isLoading = false;
          this.cdr.markForCheck();
        }
      });
  }
}
