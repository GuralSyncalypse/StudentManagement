import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CourseSectionService } from '../../../core/services/course-section.services';
import { CourseSection, CreateCourseSectionDto, Semester } from '../../../core/models/course.model';

export interface GroupedCourse {
  courseID: string;
  courseName: string;
  credits: number;
}

@Injectable({
  providedIn: 'root'
})
export class SemesterService {
  private http = inject(HttpClient);
  private apiUrl = 'api/semesters';
  private courseSectionService = inject(CourseSectionService);

  getSemesters(): Observable<Semester[]> {
    return this.http.get<Semester[]>(this.apiUrl);
  }

  toggleRegistration(semesterID: number, isEnabled: boolean): Observable<Semester> {
    return this.http.patch<Semester>(`${this.apiUrl}/${semesterID}/toggle-registration`, {
      isRegistrationEnabled: isEnabled
    });
  }

  // --- CÁC HÀM XỬ LÝ LỚP HỌC PHẦN --- //

  // 1. Lấy các lớp học phần của 1 môn trong 1 học kỳ
  getSectionsByCourse(semesterId: number, courseId: string): Observable<CourseSection[]> {
    return this.http.get<CourseSection[]>(`${this.apiUrl}/${semesterId}/courses/${courseId}/sections`);
  }

  // 2. Mở thêm lớp học phần
  addSection(courseSection: CreateCourseSectionDto): Observable<CreateCourseSectionDto> {
    return this.courseSectionService.createCourseSection(courseSection);
  }

  // 3. Xóa lớp học phần
  deleteSection(sectionId: number): Observable<void> {
    return this.courseSectionService.deleteCourseSection(sectionId);
  }

  // 4. Bật/Tắt đăng ký lớp học phần
  toggleSectionStatus(sectionId: number, isOpen: boolean): Observable<CourseSection> {
    return this.courseSectionService.toggleSectionStatus(sectionId, isOpen);
  }
}
