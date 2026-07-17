import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CourseSectionDto {
  sectionID: number;
  courseID: string;
  courseName: string;
  credits?: number;

  // Thuộc tính học kỳ phẳng hóa
  semesterID: number;
  semesterNo: number;
  startYear: number;
  academicYear: string;
  semesterDisplayName: string;

  classSection: string;
  maxCapacity: number;
  status: string;
  currentEnrollment: number;
  isEnrolled: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class CourseRegistrationService {
  private apiUrl = 'api/StudentRegistrations';

  constructor(private http: HttpClient) { }

  getAvailableSections(): Observable<CourseSectionDto[]> {
    return this.http.get<CourseSectionDto[]>(`${this.apiUrl}`);
  }

  registerCourse(sectionID: number): Observable<any> {
    return this.http.post(`${this.apiUrl}`, { sectionID });
  }

  dropCourse(sectionID: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${sectionID}`);
  }
}
