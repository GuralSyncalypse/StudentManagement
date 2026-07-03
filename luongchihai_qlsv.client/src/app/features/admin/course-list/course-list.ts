import { Component, inject, ChangeDetectorRef } from '@angular/core';
import { Router, RouterModule} from '@angular/router'
import { AdminCourseService } from './course-list.service'
import { Course } from '../../../core/models/course.model'

@Component({
  selector: 'app-course-list',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './course-list.html',
  styleUrl: './course-list.css',
})
export class CourseList {
  private studentService = inject(AdminCourseService);
  private cdr = inject(ChangeDetectorRef);

  courses: Course[] = [];

  ngOnInit(): void {
    this.loadCourses();
  }

  loadCourses() {
    this.studentService.getCourses()
      .subscribe({
        next: (res) => {
          this.courses = res;

          // 🔥 ÉP Angular update UI
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.log(err);
        }
      });
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
