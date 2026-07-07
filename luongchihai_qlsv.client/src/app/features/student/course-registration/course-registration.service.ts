import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CourseSectionDto {
  sectionID: number;
  courseID: number;
  semester: string;
  classSection: string;
  maxCapacity: number;
  status: string;
  courseName: string;
  currentEnrollment: number;
  isEnrolled: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class CourseRegistrationService {
  private apiUrl = 'api/student'; // Base URL kết nối tới Student API ở Backend

  constructor(private http: HttpClient) { }

  // Lấy danh sách các lớp học phần mở đăng ký
  getAvailableSections(): Observable<CourseSectionDto[]> {
    return this.http.get<CourseSectionDto[]>(`${this.apiUrl}/course-sections`);
  }

  // Gửi yêu cầu đăng ký học phần (Chỉ truyền lên SectionID)
  registerCourse(sectionID: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/course-sections`, { sectionID });
  }

  // Gửi yêu cầu huy đăng ký học phần (Chỉ truyền lên SectionID)
  dropCourse(sectionID: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/course-sections/${sectionID}`);
  }
}
