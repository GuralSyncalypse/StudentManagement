import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

interface Course {
  id: string;
  name: string;
  grade: string;
  attendance: string;
}

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class StudentDashboardComponent {
  // Mock data for the student profile
  student = {
    name: 'Alex Mercer',
    id: 'STU-2026-894',
    major: 'Computer Science',
    gpa: '3.82',
    semester: 'Spring 2026',
    courses: [
      { id: 'CS-301', name: 'Advanced Web Architectures', grade: 'A', attendance: '95%' },
      { id: 'CS-305', name: 'Artificial Intelligence & Ethics', grade: 'A-', attendance: '92%' },
      { id: 'MATH-210', name: 'Linear Algebra', grade: 'B+', attendance: '88%' },
      { id: 'ENG-202', name: 'Technical Writing', grade: 'A', attendance: '100%' }
    ] as Course[]
  };
}
