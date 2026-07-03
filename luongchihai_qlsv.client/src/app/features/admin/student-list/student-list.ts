import { Component, inject, ChangeDetectorRef } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { AdminStudentService } from './student-list.service'
import { StudentResponse } from '../../../core/models/student.model'

@Component({
  selector: 'app-student-list',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './student-list.html',
  styleUrl: './student-list.css',
})
export class StudentList {
  private studentService = inject(AdminStudentService);
  private cdr = inject(ChangeDetectorRef);

  students: StudentResponse[] = [];

  ngOnInit(): void {
    this.loadStudents();
  }

  loadStudents() {
    this.studentService.getStudents()
      .subscribe({
        next: (res) => {
          this.students = res;

          // 🔥 ÉP Angular update UI
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.log(err);
        }
      });
  }

  deleteStudent(id: string) {
    const confirmDelete = confirm('Bạn có chắc chắn muốn xoá sinh viên này không?');

    if (!confirmDelete) return;

    this.studentService.deleteStudent(id)
      .subscribe({
        next: () => {
          this.loadStudents();
        },
        error: (err) => {
          console.log(err);
        }
      });
  }
}
