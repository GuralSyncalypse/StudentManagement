import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { AdminCourseService } from '../course-list.service';
import { Course } from '../../../../core/models/course.model';
@Component({
  selector: 'app-form-component',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './form-component.html',
  styleUrl: './form-component.css',
})
export class CourseFormComponent {
  private fb = inject(FormBuilder);
  private courseService = inject(AdminCourseService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  courseForm!: FormGroup;
  isEditMode = false;
  courseId!: string;

  ngOnInit(): void {
    this.initForm();

    this.courseId = this.route.snapshot.paramMap.get('id')!;

    if (this.courseId) {
      this.isEditMode = true;
      this.loadCourse(this.courseId);
    }
  }

  initForm() {
    this.courseForm = this.fb.group({
      courseID: ['', Validators.required],
      courseName: ['', Validators.required],
      credits: [null, Validators.required]
    });
  }

  loadCourse(id: string) {
    this.courseService.getCourse(id).subscribe(res => {
      this.courseForm.patchValue(res);
    });
  }

  onSubmit() {
    if (this.courseForm.invalid) return;

    const data: Course = this.courseForm.value;

    if (this.isEditMode) {
      this.courseService.updateCourse(this.courseId, data)
        .subscribe(() => this.router.navigate(['/admin/courses']));
    } else {
      this.courseService.createCourse(data)
        .subscribe(() => this.router.navigate(['/admin/courses']));
    }
  }
    
  resetForm() {
    this.courseForm.reset();
  }
}
