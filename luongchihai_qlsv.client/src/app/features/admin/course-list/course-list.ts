import { Component, inject, ChangeDetectorRef, OnInit } from '@angular/core';
import { RouterModule} from '@angular/router'
import { FormsModule } from '@angular/forms';
import { AdminCourseService } from './course-list.service'
import { Course } from '../../../core/models/course.model'

@Component({
  selector: 'app-course-list',
  standalone: true,
  imports: [RouterModule, FormsModule],
  templateUrl: './course-list.html',
  styleUrl: './course-list.css',
})
export class CourseList implements OnInit {
  private studentService = inject(AdminCourseService);
  private cdr = inject(ChangeDetectorRef);

  allCourses: Course[] = [];
  courses: Course[] = [];
  searchTerm = '';
  selectedCredits = 'All';
  creditOptions: number[] = [];

  ngOnInit(): void {
    this.loadCourses();
  }

  loadCourses() {
    this.studentService.getCourses()
      .subscribe({
        next: (res) => {
          this.allCourses = res;
          this.creditOptions = this.getUniqueCredits(res);
          this.applyFilters();
        },
        error: (err) => {
          console.log(err);
        }
      });
  }

  onSearch(value: string): void {
    this.searchTerm = value.trim().toLowerCase();
    this.applyFilters();
  }

  onCreditsChange(value: string): void {
    this.selectedCredits = value;
    this.applyFilters();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedCredits = 'All';
    this.applyFilters();
  }

  applyFilters(): void {
    const normalizedSearch = this.searchTerm;

    this.courses = this.allCourses.filter(course => {
      const matchSearch = !normalizedSearch || [
        course.courseID,
        course.courseName
      ].some(value => value?.toLowerCase().includes(normalizedSearch));

      const matchCredits = this.selectedCredits === 'All' || String(course.credits) === this.selectedCredits;

      return matchSearch && matchCredits;
    });

    this.cdr.detectChanges();
  }

  private getUniqueCredits(courses: Course[]): number[] {
    return [...new Set(courses.map(course => course.credits))].sort((a, b) => a - b);
  }

  deleteCourse(id: string) {
    const confirmDelete = confirm('Bạn có chắc chắn muốn xoá học phần này không?');

    if (!confirmDelete) return;

    this.studentService.deleteCourse(id)
      .subscribe({
        next: () => {
          this.loadCourses();
        },
        error: (err) => {
          console.log(err);
        }
      });
  }
}
