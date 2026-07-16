import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { StudentDetailDto } from '../../core/models/student.model';

@Injectable({ providedIn: 'root' })
export class StudentService {
  private http = inject(HttpClient);
  private apiUrl = 'api/Students';

  // For the logged-in student's own dashboard
  getMyProfile(): Observable<StudentDetailDto> {
    return this.http.get<StudentDetailDto>(`${this.apiUrl}/me`);
  }

}
