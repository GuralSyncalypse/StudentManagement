import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { SemesterService } from '../semester-list.service';
import { CourseSection, CreateCourseSectionDto } from '../../../../core/models/course.model';

@Component({
  selector: 'app-section-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './section-list.html'
})
export class SectionListComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private semesterService = inject(SemesterService);
  private location = inject(Location);

  semesterId = signal<number>(0);
  courseId = signal<string>('');

  sections = signal<CourseSection[]>([]);
  isLoading = signal<boolean>(true);
  error = signal<string | null>(null);

  // --- State Form theo CreateCourseSectionDto ---
  isAddModalOpen = signal<boolean>(false);
  newClassSection = signal<string>('L01');
  newMaxCapacity = signal<number | null>(40);
  newStatus = signal<string>('Open');

  ngOnInit(): void {
    const sId = Number(this.route.snapshot.paramMap.get('semesterId'));
    const cId = this.route.snapshot.paramMap.get('courseId') || '';

    this.semesterId.set(sId);
    this.courseId.set(cId);

    this.loadSections();
  }

  loadSections(): void {
    this.isLoading.set(true);
    this.semesterService.getSectionsByCourse(this.semesterId(), this.courseId()).subscribe({
      next: (data) => {
        this.sections.set(data ?? []);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.error.set('Không thể tải danh sách lớp học phần.');
        this.isLoading.set(false);
      }
    });
  }

  goBack(): void {
    this.location.back();
  }

  openAddModal(): void {
    const count = this.sections().length + 1;
    const autoSectionName = count < 10 ? `L0${count}` : `L${count}`;

    this.newClassSection.set(autoSectionName);
    this.newMaxCapacity.set(40);
    this.newStatus.set('Open');
    this.isAddModalOpen.set(true);
  }

  closeAddModal(): void {
    this.isAddModalOpen.set(false);
  }

  confirmAddSection(): void {
    if (!this.newClassSection().trim()) return;

    const dto: CreateCourseSectionDto = {
      courseID: this.courseId(),
      semesterID: this.semesterId(),
      classSection: this.newClassSection().trim(),
      maxCapacity: this.newMaxCapacity(),
      status: this.newStatus()
    };

    this.semesterService.addSection(dto).subscribe({
      next: () => {
        this.loadSections();
        this.closeAddModal();
      },
      error: (err) => {
        console.error('Lỗi tạo lớp học phần:', err);
        alert('Không thể tạo lớp học phần mới!');
      }
    });
  }

  // 2. XOÁ LỚP (Sửa sectionName -> classSection)
  onDeleteSection(section: CourseSection): void {
    if (!confirm(`Bạn có chắc chắn muốn xóa lớp "${section.classSection}"?`)) return;

    this.semesterService.deleteSection(section.sectionID).subscribe({
      next: () => {
        this.sections.update(list => list.filter(s => s.sectionID !== section.sectionID));
      },
      error: () => alert('Không thể xóa lớp này!')
    });
  }

  // 3. ĐÓNG / MỞ ĐĂNG KÝ (Sửa isOpen -> status)
  onToggleStatus(section: CourseSection): void {
    const isCurrentlyOpen = section.status === 'Open';
    const newIsOpenBool = !isCurrentlyOpen;
    const newStatusStr = newIsOpenBool ? 'Open' : 'Closed';

    this.semesterService.toggleSectionStatus(section.sectionID, newIsOpenBool).subscribe({
      next: () => {
        this.sections.update(list =>
          list.map(s => s.sectionID === section.sectionID ? { ...s, status: newStatusStr } : s)
        );
      },
      error: () => alert('Không thể đổi trạng thái đăng ký!')
    });
  }
}
