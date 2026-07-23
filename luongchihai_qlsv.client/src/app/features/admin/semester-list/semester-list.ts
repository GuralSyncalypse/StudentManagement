import {
  Component,
  inject,
  OnInit,
  signal,
  computed,
  DestroyRef,
  ChangeDetectionStrategy
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule, DatePipe } from '@angular/common';
import {
  FormsModule,
  ReactiveFormsModule,
  FormBuilder,
  Validators,
  FormGroup,
  AbstractControl,
  ValidationErrors,
  ValidatorFn
} from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { throttleTime } from 'rxjs/operators';
import { CourseSection, Semester } from '../../../core/models/course.model';
import { SemesterService, GroupedCourse, CreateSemesterRequest } from './semester-list.service';

// --- INTERFACES CHO POPUPS ---
export interface ToastNotification {
  id: number;
  message: string;
  type: 'success' | 'error' | 'info';
}

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  action: () => void;
}

// --- CUSTOM VALIDATORS ---
export function dateRangeValidator(startKey: string, endKey: string, errorKey: string): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const start = control.get(startKey)?.value;
    const end = control.get(endKey)?.value;
    if (start && end && new Date(start) >= new Date(end)) {
      return { [errorKey]: true };
    }
    return null;
  };
}

export function yearMatchValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const startYear = control.get('startYear')?.value;
    const startDate = control.get('startDate')?.value;

    if (startYear && startDate) {
      const yearFromDate = new Date(startDate).getFullYear();
      if (Number(startYear) !== yearFromDate) {
        return { yearMismatch: true };
      }
    }
    return null;
  };
}

@Component({
  selector: 'app-semester-list',
  standalone: true,
  imports: [CommonModule, DatePipe, FormsModule, ReactiveFormsModule],
  templateUrl: './semester-list.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SemesterListComponent implements OnInit {
  private readonly semesterService = inject(SemesterService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);

  // --- STATE SIGNALS ---
  readonly semesters = signal<Semester[]>([]);
  readonly isLoading = signal<boolean>(true);
  readonly error = signal<string | null>(null);
  readonly updatingSemesterId = signal<number | null>(null);

  readonly searchTerm = signal<string>('');
  readonly selectedStatus = signal<'all' | 'open' | 'closed'>('all');
  readonly selectedYear = signal<number | 'all'>('all');
  readonly expandedSemesterId = signal<number | null>(null);
  readonly deletingSemesterId = signal<number | null>(null);

  // Combobox Dropdown State
  readonly isYearDropdownOpen = signal<boolean>(false);
  readonly yearSearchTerm = signal<string>('');

  // Modal "Thêm mới" State Signal
  readonly isAddModalOpen = signal<boolean>(false);
  readonly isSubmitting = signal<boolean>(false);

  // --- POPUP SIGNALS (MỚI) ---
  readonly toast = signal<ToastNotification | null>(null);
  readonly confirmModalData = signal<ConfirmDialogData | null>(null);

  // --- REACTIVE FORM ---
  readonly semesterForm: FormGroup = this.fb.group({
    semesterNo: [1, [Validators.required, Validators.min(1), Validators.max(10)]],
    startYear: [new Date().getFullYear(), [Validators.required, Validators.min(2000), Validators.max(2100)]],
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
    registrationStartDate: [''],
    registrationEndDate: [''],
    isRegistrationEnabled: [true]
  }, {
    validators: [
      yearMatchValidator(),
      dateRangeValidator('startDate', 'endDate', 'invalidStudyDates'),
      dateRangeValidator('registrationStartDate', 'registrationEndDate', 'invalidRegDates')
    ]
  });

  private readonly toggleSubject = new Subject<{ semester: Semester; event: Event }>();

  constructor() {
    this.toggleSubject.pipe(
      throttleTime(600),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(({ semester, event }) => {
      this.executeToggleRegistration(semester, event);
    });
  }

  // --- HELPER QUẢN LÝ SNACKBAR (TOAST) ---
  showToast(message: string, type: 'success' | 'error' | 'info' = 'success'): void {
    const id = Date.now();
    this.toast.set({ id, message, type });
    setTimeout(() => {
      if (this.toast()?.id === id) {
        this.toast.set(null);
      }
    }, 3500);
  }

  closeToast(): void {
    this.toast.set(null);
  }

  // --- HELPER QUẢN LÝ DIALOG XÁC NHẬN ---
  openConfirmDialog(data: ConfirmDialogData): void {
    this.confirmModalData.set(data);
  }

  closeConfirmDialog(): void {
    this.confirmModalData.set(null);
  }

  executeConfirmAction(): void {
    const modal = this.confirmModalData();
    if (modal) {
      modal.action();
      this.closeConfirmDialog();
    }
  }

  // COMPUTED SIGNALS
  readonly availableYears = computed<number[]>(() => {
    const years = this.semesters().map(s => s.startYear);
    return Array.from(new Set(years)).sort((a, b) => b - a);
  });

  readonly groupedYears = computed(() => {
    const term = this.yearSearchTerm().trim().toLowerCase();
    const allYears = this.availableYears();

    const filtered = allYears.filter(year => {
      const yearLabel = `năm học ${year} - ${year + 1}`;
      return yearLabel.toLowerCase().includes(term) || year.toString().includes(term);
    });

    const recent = filtered.filter(year => allYears.indexOf(year) < 5);
    const older = filtered.filter(year => allYears.indexOf(year) >= 5);

    return { recent, older, totalCount: filtered.length };
  });

  readonly selectedYearLabel = computed<string>(() => {
    const year = this.selectedYear();
    return year === 'all' ? 'Tất cả năm học' : `Năm học ${year} - ${year + 1}`;
  });

  readonly filteredSemesters = computed<Semester[]>(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const status = this.selectedStatus();
    const year = this.selectedYear();

    return this.semesters().filter(semester => {
      const matchesStatus =
        status === 'all' ||
        (status === 'open' && semester.isRegistrationEnabled) ||
        (status === 'closed' && !semester.isRegistrationEnabled);

      const matchesYear = year === 'all' || semester.startYear === year;
      const sections = semester.courseSections ?? [];
      const matchesSearch = !term ||
        `học kỳ ${semester.semesterNo}`.toLowerCase().includes(term) ||
        sections.some(cs =>
          cs.courseID.toLowerCase().includes(term) ||
          (cs.courseName && cs.courseName.toLowerCase().includes(term))
        );

      return matchesStatus && matchesYear && matchesSearch;
    });
  });

  ngOnInit(): void {
    this.loadSemesters();
  }

  loadSemesters(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.semesterService.getSemesters().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (data) => {
        this.semesters.set(data ?? []);
        this.isLoading.set(false);
        if (data && data.length > 0) {
          this.expandedSemesterId.set(data[0].semesterID);
        }
      },
      error: (err) => {
        console.error('API Error:', err);
        this.error.set('Không thể kết nối đến hệ thống!');
        this.isLoading.set(false);
      }
    });
  }

  toggleYearDropdown(): void {
    this.isYearDropdownOpen.update(open => !open);
    if (!this.isYearDropdownOpen()) {
      this.yearSearchTerm.set('');
    }
  }

  selectYearOption(year: number | 'all'): void {
    this.selectedYear.set(year);
    this.isYearDropdownOpen.set(false);
    this.yearSearchTerm.set('');
  }

  toggleExpand(semesterID: number): void {
    this.expandedSemesterId.update(id => id === semesterID ? null : semesterID);
  }

  onToggleRegistration(semester: Semester, event: Event): void {
    event.stopPropagation();
    if (this.updatingSemesterId() === semester.semesterID) return;
    this.toggleSubject.next({ semester, event });
  }

  private executeToggleRegistration(semester: Semester, event: Event): void {
    const newStatus = !semester.isRegistrationEnabled;
    this.updatingSemesterId.set(semester.semesterID);

    this.semesterService.toggleRegistration(semester.semesterID, newStatus).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.semesters.update(list =>
          list.map(s => s.semesterID === semester.semesterID ? { ...s, isRegistrationEnabled: newStatus } : s)
        );
        this.updatingSemesterId.set(null);
        this.showToast(`Đã ${newStatus ? 'mở' : 'đóng'} đăng ký cho Học kỳ ${semester.semesterNo}!`, 'success');
      },
      error: () => {
        this.updatingSemesterId.set(null);
        this.showToast('Không thể cập nhật trạng thái đăng ký!', 'error');
      }
    });
  }

  // --- XÓA HỌC KỲ BẰNG DIALOG & SNACKBAR ---
  onDeleteSemester(semester: Semester, event: Event): void {
    event.stopPropagation();

    if (this.deletingSemesterId() === semester.semesterID) return;

    // Mở Confirm Dialog thay vì dùng confirm() trình duyệt
    this.openConfirmDialog({
      title: 'Xóa Học Kỳ',
      message: `Bạn có chắc chắn muốn xóa "Học kỳ ${semester.semesterNo} (Năm ${semester.startYear} - ${semester.startYear + 1})" không? Thao tác này không thể hoàn tác!`,
      confirmText: 'Xóa ngay',
      cancelText: 'Hủy bỏ',
      action: () => this.executeDeleteSemester(semester)
    });
  }

  private executeDeleteSemester(semester: Semester): void {
    this.deletingSemesterId.set(semester.semesterID);

    this.semesterService.deleteSemester(semester.semesterID).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.semesters.update(list => list.filter(s => s.semesterID !== semester.semesterID));
        if (this.expandedSemesterId() === semester.semesterID) {
          this.expandedSemesterId.set(null);
        }
        this.deletingSemesterId.set(null);
        this.showToast(`Đã xóa Học kỳ ${semester.semesterNo} thành công!`, 'success');
      },
      error: (err) => {
        console.error('Lỗi khi xóa học kỳ:', err);
        this.deletingSemesterId.set(null);
        this.showToast('Không thể xóa! Học kỳ đã có lớp học phần hoặc dữ liệu liên quan.', 'error');
      }
    });
  }

  // --- TẠO HỌC KỲ MỚI ---
  onAddSemester(): void {
    const currentYear = new Date().getFullYear();
    this.semesterForm.reset({
      semesterNo: 1,
      startYear: currentYear,
      startDate: '',
      endDate: '',
      registrationStartDate: '',
      registrationEndDate: '',
      isRegistrationEnabled: true
    });
    this.isAddModalOpen.set(true);
  }

  onStartDateChange(): void {
    const startDateVal = this.semesterForm.get('startDate')?.value;
    if (startDateVal) {
      const year = new Date(startDateVal).getFullYear();
      if (!isNaN(year)) {
        this.semesterForm.patchValue({ startYear: year }, { emitEvent: false });
        this.semesterForm.updateValueAndValidity();
      }
    }
  }

  closeAddModal(): void {
    if (this.isSubmitting()) return;
    this.isAddModalOpen.set(false);
  }

  onSubmitSemester(): void {
    if (this.semesterForm.invalid) {
      this.semesterForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const formVal = this.semesterForm.value;

    const request: CreateSemesterRequest = {
      semesterNo: Number(formVal.semesterNo),
      startYear: Number(formVal.startYear),
      startDate: formVal.startDate,
      endDate: formVal.endDate,
      registrationStartDate: formVal.registrationStartDate || null,
      registrationEndDate: formVal.registrationEndDate || null,
      isRegistrationEnabled: !!formVal.isRegistrationEnabled
    };

    this.semesterService.createSemester(request).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (newSemester) => {
        this.semesters.update(list => [newSemester, ...list]);
        this.expandedSemesterId.set(newSemester.semesterID);
        this.isSubmitting.set(false);
        this.closeAddModal();
        this.showToast(`Đã tạo thành công Học kỳ ${newSemester.semesterNo}!`, 'success');
      },
      error: (err) => {
        console.error('Lỗi khi tạo học kỳ:', err);
        this.isSubmitting.set(false);
        this.showToast('Có lỗi xảy ra khi tạo học kỳ mới!', 'error');
      }
    });
  }

  getGroupedCourses(sections?: CourseSection[] | null): GroupedCourse[] {
    if (!sections || !Array.isArray(sections)) return [];
    const map = new Map<string, CourseSection>();
    sections.forEach(sec => {
      if (!map.has(sec.courseID)) map.set(sec.courseID, sec);
    });

    return Array.from(map.values()).map(sec => ({
      courseID: sec.courseID,
      courseName: sec.courseName || 'Tên môn học chưa cập nhật',
      credits: sec.credits ?? 3
    }));
  }

  getTotalCredits(semester: Semester): number {
    const courses = this.getGroupedCourses(semester.courseSections);
    return courses.reduce((sum, c) => sum + (c.credits || 0), 0);
  }

  isCurrentSemester(semester: Semester): boolean {
    const now = new Date();
    if (semester.startDate && semester.endDate) {
      const start = new Date(semester.startDate);
      const end = new Date(semester.endDate);
      return now >= start && now <= end;
    }
    const maxYear = Math.max(...this.semesters().map(s => s.startYear));
    return semester.startYear === maxYear && semester.semesterNo === 1;
  }

  goToCourseSections(semesterId: number, courseId: string): void {
    this.router.navigate([semesterId, 'courses', courseId], {
      relativeTo: this.route
    });
  }

  onAddCourseSection(semesterID: number, event: Event): void {
    event.stopPropagation();
    this.showToast(`Mở Form thêm lớp học phần cho Học kỳ ID: ${semesterID}`, 'info');
  }
}
