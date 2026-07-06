import { Component, inject, ChangeDetectorRef, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AdminStudentService } from './student-list.service'
import { StudentResponse } from '../../../core/models/student.model'

@Component({
  selector: 'app-student-list',
  standalone: true,
  imports: [RouterModule, FormsModule],
  templateUrl: './student-list.html',
  styleUrl: './student-list.css',
})
export class StudentList implements OnInit {
  private studentService = inject(AdminStudentService);
  private cdr = inject(ChangeDetectorRef);

  allStudents: StudentResponse[] = [];
  students: StudentResponse[] = [];
  facultyOptions: string[] = [];
  searchTerm = '';
  selectedGender = 'All';
  selectedFaculty = 'All';

  ngOnInit(): void {
    this.loadStudents();
  }

  loadStudents() {
    this.studentService.getStudents()
      .subscribe({
        next: (res) => {
          this.allStudents = res;
          this.facultyOptions = this.getUniqueValues(
            res.map(student => student.academicProfile?.facultyName).filter((value): value is string => !!value)
          );
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

  onGenderChange(value: string): void {
    this.selectedGender = value;
    this.applyFilters();
  }

  onFacultyChange(value: string): void {
    this.selectedFaculty = value;
    this.applyFilters();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedGender = 'All';
    this.selectedFaculty = 'All';
    this.applyFilters();
  }

  applyFilters(): void {
    const normalizedSearch = this.searchTerm;

    this.students = this.allStudents.filter(student => {
      const profile = student.academicProfile;
      const matchSearch = !normalizedSearch || [
        student.studentID,
        student.studentName,
        student.gender,
        student.ethnicity,
        student.permanentAddress,
        profile?.className,
        profile?.facultyName,
        profile?.majorName
      ].some(value => value?.toLowerCase().includes(normalizedSearch));

      const matchGender = this.selectedGender === 'All' || student.gender === this.selectedGender;
      const matchFaculty = this.selectedFaculty === 'All' || profile?.facultyName === this.selectedFaculty;

      return matchSearch && matchGender && matchFaculty;
    });

    this.cdr.detectChanges();
  }

  private getUniqueValues(values: string[]): string[] {
    return [...new Set(values)].sort((a, b) => a.localeCompare(b, 'vi'));
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
