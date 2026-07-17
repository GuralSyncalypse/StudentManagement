import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CourseRegistrationService, CourseSectionDto } from './course-registration.service';

interface SemesterFilter {
  semesterID: number;
  semesterDisplayName: string;
  academicYear: string;
}

export interface CourseGroup {
  courseID: string;
  courseName: string;
  credits: number;
  semesterDisplayName: string;
  isExpanded: boolean;
  hasEnrolled: boolean;
  enrolledSectionCode?: string;
  sections: CourseSectionDto[];
}

@Component({
  selector: 'app-course-registration',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './course-registration.html',
  styleUrls: ['./course-registration.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CourseRegistrationComponent implements OnInit {
  allSections: CourseSectionDto[] = [];
  courseGroups: CourseGroup[] = [];
  uniqueSemesters: SemesterFilter[] = [];

  selectedSemesterId: string = 'All';
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  totalRegisteredCourses = 0;
  totalRegisteredCredits = 0;

  // --- TRẠNG THÁI CHO POPUP & SNACKBAR MỚI ---
  isConfirmOpen = false;
  confirmActionType: 'register' | 'drop' | null = null;
  selectedSectionForAction: CourseSectionDto | null = null;
  private toastTimeout: any;

  constructor(
    private registrationService: CourseRegistrationService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadSections();
  }

  loadSections(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.cdr.markForCheck();

    const expandedCourseIds = new Set<string>(
      this.courseGroups.filter(g => g.isExpanded).map(g => g.courseID)
    );

    this.registrationService.getAvailableSections().subscribe({
      next: (data) => {
        this.allSections = data;
        this.extractUniqueSemesters(data);
        this.applyGroupingAndCalculations(expandedCourseIds);

        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.showToast('Không thể tải danh sách học phần. Vui lòng thử lại sau!', true);
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  // --- KIỂM TRA HỌC KỲ ĐÃ ĐƯỢC ĐĂNG KÝ LỚP NÀO CHƯA ---
  isCourseInSemesterLocked(courseID: string, semesterID: number): boolean {
    // Trả về true nếu trong học kỳ này, học phần (courseID) đã có ít nhất một lớp được đăng ký (isEnrolled = true)
    return this.allSections.some(section =>
      section.courseID === courseID &&
      section.semesterID === semesterID &&
      section.isEnrolled
    );
  }

  private extractUniqueSemesters(data: CourseSectionDto[]): void {
    const seen = new Set<number>();
    this.uniqueSemesters = [];

    data.forEach(item => {
      if (item.semesterID && !seen.has(item.semesterID)) {
        seen.add(item.semesterID);
        this.uniqueSemesters.push({
          semesterID: item.semesterID,
          semesterDisplayName: item.semesterDisplayName,
          academicYear: item.academicYear
        });
      }
    });
    this.uniqueSemesters.sort((a, b) => b.semesterID - a.semesterID);
  }

  onFilterSemester(event: Event): void {
    this.selectedSemesterId = (event.target as HTMLSelectElement).value;
    this.applyGroupingAndCalculations();
  }

  private applyGroupingAndCalculations(previouslyExpandedIds?: Set<string>): void {
    const filteredRaw = this.selectedSemesterId === 'All'
      ? this.allSections
      : this.allSections.filter(s => s.semesterID === +this.selectedSemesterId);

    const groupsMap = new Map<string, CourseGroup>();

    filteredRaw.forEach(section => {
      if (!groupsMap.has(section.courseID)) {
        groupsMap.set(section.courseID, {
          courseID: section.courseID,
          courseName: section.courseName,
          credits: section.credits || 3,
          semesterDisplayName: section.semesterDisplayName,
          isExpanded: previouslyExpandedIds ? previouslyExpandedIds.has(section.courseID) : false,
          hasEnrolled: false,
          sections: []
        });
      }

      const group = groupsMap.get(section.courseID)!;
      group.sections.push(section);

      if (section.isEnrolled) {
        group.hasEnrolled = true;
        group.enrolledSectionCode = section.classSection;
      }
    });

    this.courseGroups = Array.from(groupsMap.values());

    const enrolledSections = this.allSections.filter(s => s.isEnrolled);
    this.totalRegisteredCourses = enrolledSections.length;
    this.totalRegisteredCredits = enrolledSections.reduce(
      (sum, s) => sum + (s.credits || 3), 0
    );
  }

  toggleExpand(group: CourseGroup): void {
    group.isExpanded = !group.isExpanded;
    this.cdr.markForCheck();
  }

  // --- CÁC HÀM ĐƯỢC THAY THẾ CHO TRẢI NGHIỆM POPUP MỚI ---

  // Khi click nút Đăng Ký
  onRegister(section: CourseSectionDto): void {
    this.selectedSectionForAction = section;
    this.confirmActionType = 'register';
    this.isConfirmOpen = true;
    this.cdr.markForCheck();
  }

  // Khi click nút Hủy Đăng Ký
  onDrop(section: CourseSectionDto): void {
    this.selectedSectionForAction = section;
    this.confirmActionType = 'drop';
    this.isConfirmOpen = true;
    this.cdr.markForCheck();
  }

  // Đóng Popup xác nhận
  closeConfirm(): void {
    this.isConfirmOpen = false;
    this.selectedSectionForAction = null;
    this.confirmActionType = null;
    this.cdr.markForCheck();
  }

  // Thực thi tác vụ sau khi người dùng ấn xác nhận trên Popup
  confirmAction(): void {
    if (!this.selectedSectionForAction || !this.confirmActionType) return;

    const section = this.selectedSectionForAction;
    const type = this.confirmActionType;

    this.closeConfirm(); // Đóng nhanh popup trước khi thực thi
    this.isLoading = true;
    this.cdr.markForCheck();

    if (type === 'register') {
      this.registrationService.registerCourse(section.sectionID).subscribe({
        next: () => {
          this.showToast(`Đăng ký thành công lớp ${section.classSection} môn ${section.courseName}!`);
          this.loadSections();
        },
        error: (err) => {
          this.showToast(err.error?.message || 'Đăng ký học phần thất bại. Vui lòng kiểm tra lại!', true);
          this.isLoading = false;
          this.cdr.markForCheck();
        }
      });
    } else if (type === 'drop') {
      this.registrationService.dropCourse(section.sectionID).subscribe({
        next: () => {
          this.showToast(`Đã hủy đăng ký thành công lớp ${section.classSection} môn ${section.courseName}.`);
          this.loadSections();
        },
        error: (err) => {
          this.showToast(err.error?.message || 'Hủy đăng ký thất bại. Vui lòng thử lại!', true);
          this.isLoading = false;
          this.cdr.markForCheck();
        }
      });
    }
  }

  // Quản lý hiển thị Snackbar thông minh
  showToast(message: string, isError = false): void {
    if (this.toastTimeout) {
      clearTimeout(this.toastTimeout);
    }

    if (isError) {
      this.errorMessage = message;
      this.successMessage = '';
    } else {
      this.successMessage = message;
      this.errorMessage = '';
    }
    this.cdr.markForCheck();

    // Tự động đóng thông báo sau 4 giây
    this.toastTimeout = setTimeout(() => {
      this.clearAlerts();
    }, 4000);
  }

  clearAlerts(): void {
    this.successMessage = '';
    this.errorMessage = '';
    this.cdr.markForCheck();
  }
}
