import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { EnrollmentService } from './enrollment.services';
import { AvailableCourseSection } from '../models/course.model';
import { CourseSection, Semester, CreateCourseSectionDto } from '../models/course.model';

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
  createCourseSection(courseSection: CreateCourseSectionDto): Observable<CreateCourseSectionDto> {
    return this.http.post<CreateCourseSectionDto>(this.apiUrl, courseSection);
  }

  // UPDATE
  updateCourseSection(id: number, courseSection: AvailableCourseSection): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, courseSection);
  }

  // DELETE
  deleteCourseSection(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  toggleSectionStatus(sectionId: number, isOpen: boolean): Observable<CourseSection> {
    return this.http.patch<CourseSection>(`${this.apiUrl}/${sectionId}/toggle-status`, { isOpen });
  }
}
