import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { AdminStudentService } from '../student-list.service';
import { StudentDetailDto } from '../../../../core/models/student.model';

@Component({
  selector: 'app-form.component',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './form.component.html',
  styleUrl: './form.component.css',
})
export class StudentFormComponent implements OnInit {

  private fb = inject(FormBuilder);
  private studentService = inject(AdminStudentService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  studentForm!: FormGroup;
  isEditMode = false;
  studentId!: string;

  ngOnInit(): void {
    this.initForm();

    this.studentId = this.route.snapshot.paramMap.get('id') || '';

    if (this.studentId) {
      this.isEditMode = true;
      this.loadStudent(this.studentId);
    }
  }

  initForm() {
    this.studentForm = this.fb.group({
      studentID: ['', Validators.required],
      studentName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', Validators.required],

      gender: [null],
      birthDate: [null],
      ethnicity: [null],
      religion: [null],
      nationality: [null],
      birthPlace: [null],

      citizenID: [null],
      citizenIDIssueDate: [null],
      citizenIDIssuePlace: [null],

      permanentAddress: [null],
      temporaryAddress: [null]
    });
  }

  formatDateToInput(dateInput: any): string {
    if (!dateInput) return '';

    const date = new Date(dateInput);
    // Kiểm tra xem date có hợp lệ không
    if (isNaN(date.getTime())) return '';

    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0'); // Tháng chạy từ 0-11 nên phải +1
    const day = String(date.getDate()).padStart(2, '0');

    return `${year}-${month}-${day}`;
  }

  loadStudent(id: string) {
    this.studentService.getStudent(id).subscribe(res => {
      this.studentForm.patchValue({
        ...res,
        birthDate: this.formatDateToInput(res.birthDate),
        citizenIDIssueDate: this.formatDateToInput(res.citizenIDIssueDate)
      });
    });
  }

  onSubmit() {
    if (this.studentForm.invalid) return;

    const data: StudentDetailDto = this.studentForm.value;

    if (this.isEditMode) {
      this.studentService.updateStudent(this.studentId, data)
        .subscribe(() => this.router.navigate(['/admin/students']));
    } else {
      this.studentService.createStudent(data)
        .subscribe(() => this.router.navigate(['/admin/students']));
    }
  }

  resetForm() {
    this.studentForm.reset();
  }
}
