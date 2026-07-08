import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { EnrollmentService } from '../../../core/services/enrollment.services'
import { CourseSection } from '../../../core/models/course.model';

@Injectable({
  providedIn: 'root'
})
export class CourseSectionService {
  private http = inject(HttpClient)
  private enrollmentService = inject(EnrollmentService);
  private apiUrl = '/api/CourseSections';

  // GET ALL
  getCourseSections(): Observable<CourseSection[]> {
    return this.http.get<CourseSection[]>(this.apiUrl);
  }

  // GET BY ID
  getCourseSection(id: number): Observable<CourseSection> {
    return this.http.get<CourseSection>(`${this.apiUrl}/${id}`);
  }

  // CREATE
  createCourseSection(courseSection: CourseSection): Observable<CourseSection> {
    return this.http.post<CourseSection>(this.apiUrl, courseSection);
  }

  // UPDATE
  updateCourseSection(id: number, courseSection: CourseSection): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, courseSection);
  }

  // DELETE
  deleteCourseSection(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
