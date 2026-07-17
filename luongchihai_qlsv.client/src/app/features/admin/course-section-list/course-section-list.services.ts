import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { EnrollmentService } from '../../../core/services/enrollment.services';
// THAY ĐỔI: Cập nhật gọi interface AvailableCourseSection mở rộng
import { AvailableCourseSection } from '../../../core/models/course.model';

@Injectable({
  providedIn: 'root'
})
export class CourseSectionService {
  private http = inject(HttpClient);
  private enrollmentService = inject(EnrollmentService);
  private apiUrl = '/api/CourseSections';

  // THAY ĐỔI: Trả về AvailableCourseSection[] tương ứng Flat JSON của Backend
  getCourseSections(): Observable<AvailableCourseSection[]> {
    return this.http.get<AvailableCourseSection[]>(this.apiUrl);
  }

  // THAY ĐỔI: Get đơn lẻ cấu trúc mới
  getCourseSection(id: number): Observable<AvailableCourseSection> {
    return this.http.get<AvailableCourseSection>(`${this.apiUrl}/${id}`);
  }

  // CREATE
  createCourseSection(courseSection: AvailableCourseSection): Observable<AvailableCourseSection> {
    return this.http.post<AvailableCourseSection>(this.apiUrl, courseSection);
  }

  // UPDATE
  updateCourseSection(id: number, courseSection: AvailableCourseSection): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, courseSection);
  }

  // DELETE
  deleteCourseSection(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
