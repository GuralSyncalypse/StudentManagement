import { Component, inject, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StudentDetailDto } from '../../../core/models/student.model'
import { StudentService } from '../student-profile.service'

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StudentProfileComponent {
  private studentService = inject(StudentService);
  private cdr = inject(ChangeDetectorRef);

  // Biến thường, không phải Signal
  student?: StudentDetailDto;

  ngOnInit(): void {
    this.studentService.getMyProfile().subscribe({
      next: (data) => {
        this.student = data;

        // 🌟 QUAN TRỌNG NHẤT: Báo cho Angular biết dữ liệu đã về, hãy cập nhật UI đi!
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Could not fetch personal profile data', err);
      }
    });
  }
}
