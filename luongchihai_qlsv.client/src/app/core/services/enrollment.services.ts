import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { Enrollment } from '../models/enrollment.model';

@Injectable({
  providedIn: 'root'
})
export class EnrollmentService {
  private http = inject(HttpClient)
  private apiUrl = '/api/enrollments';

  // GET ALL
  getEnrollments(): Observable<Enrollment[]> {
    return this.http.get<Enrollment[]>(this.apiUrl);
  }

  // GET BY ID
  getEnrollment(id: number): Observable<Enrollment> {
    return this.http.get<Enrollment>(`${this.apiUrl}/${id}`);
  }

  // CREATE
  createEnrollment(enrollment: Enrollment): Observable<Enrollment> {
    return this.http.post<Enrollment>(this.apiUrl, enrollment);
  }

  // UPDATE
  updateEnrollment(id: number, enrollment: Enrollment): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, enrollment);
  }

  // DELETE
  deleteEnrollment(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  adminRegisterEnrollment(payload: { sectionID: number; studentID: string }): Observable<any> {
    // Gửi yêu cầu POST lên API endpoint xử lý đăng ký của Admin
    // Đường dẫn ví dụ: api/coursesections/admin-register
    return this.http.post<any>(`${this.apiUrl}`, payload);
  }

  adminCancelEnrollment(payload: { sectionID: number; studentID: string }): Observable<any> {
    // Truyền qua Query Parameters
    return this.http.delete<any>(`${this.apiUrl}`, {
      params: {
        sectionID: payload.sectionID.toString(),
        studentID: payload.studentID
      }
    });
  }
}
