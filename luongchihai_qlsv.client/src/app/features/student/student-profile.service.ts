import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { StudentResponse } from '../../core/models/student.model';

@Injectable({ providedIn: 'root' })
export class StudentService {
  private http = inject(HttpClient);
  private apiUrl = 'api/student';

  // For the logged-in student's own dashboard
  getMyProfile(): Observable<StudentResponse> {
    return this.http.get<StudentResponse>(`${this.apiUrl}/me`);
  }

}
