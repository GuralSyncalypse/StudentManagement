import { Component, inject, OnInit, signal, computed, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router'; // <-- Thêm Router
import { Subject } from 'rxjs';
import { throttleTime } from 'rxjs/operators';
import { CourseSection, Semester } from '../../../core/models/course.model';
import { SemesterService, GroupedCourse } from './semester-list.service';

@Component({
  selector: 'app-semester-list',
  standalone: true,
  imports: [CommonModule, DatePipe, FormsModule],
  templateUrl: './semester-list.html'
})
export class SemesterListComponent implements OnInit {
  private semesterService = inject(SemesterService);
  private destroyRef = inject(DestroyRef);
  private router = inject(Router); // <-- Inject Router
  private route = inject(ActivatedRoute);

  // Signals & State
  semesters = signal<Semester[]>([]);
  isLoading = signal<boolean>(true);
  error = signal<string | null>(null);
  updatingSemesterId = signal<number | null>(null);

  searchTerm = signal<string>('');
  selectedStatus = signal<'all' | 'open' | 'closed'>('all');
  selectedYear = signal<number | 'all'>('all');
  expandedSemesterId = signal<number | null>(null);

  private toggleSubject = new Subject<{ semester: Semester; event: Event }>();

  constructor() {
    this.toggleSubject.pipe(
      throttleTime(600),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(({ semester, event }) => {
      this.executeToggleRegistration(semester, event);
    });
  }

  availableYears = computed(() => {
    const years = this.semesters().map(s => s.startYear);
    return Array.from(new Set(years)).sort((a, b) => b - a);
  });

  filteredSemesters = computed(() => {
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

    this.semesterService.getSemesters().subscribe({
      next: (data) => {
        this.semesters.set(data ?? []);
        this.isLoading.set(false);
        if (data && data.length > 0) {
          this.expandedSemesterId.set(data[0].semesterID);
        }
      },
      error: (err) => {
        console.error('Lỗi kết nối API:', err);
        this.error.set('Không thể kết nối đến hệ thống!');
        this.isLoading.set(false);
      }
    });
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

    this.semesterService.toggleRegistration(semester.semesterID, newStatus).subscribe({
      next: () => {
        this.semesters.update(list =>
          list.map(s => s.semesterID === semester.semesterID ? { ...s, isRegistrationEnabled: newStatus } : s)
        );
        this.updatingSemesterId.set(null);
      },
      error: () => {
        alert('Không thể cập nhật trạng thái đăng ký!');
        this.updatingSemesterId.set(null);
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

  // HÀM MỚI: Điều hướng đến trang danh sách lớp học phần
  goToCourseSections(semesterId: number, courseId: string): void {
    // 3. Sử dụng relativeTo thay vì dẫn tuyệt đối
    this.router.navigate([semesterId, 'courses', courseId], {
      relativeTo: this.route
    });
  }

  onAddCourseSection(semesterID: number, event: Event): void {
    event.stopPropagation();
    alert(`Mở Form thêm lớp học phần mới cho Học kỳ ID: ${semesterID}`);
  }
}
