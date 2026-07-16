import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  StudentBaseDto,
  StudentDetailDto,
  StudentListDto
} from '../../../core/models/student.model';

@Injectable({
  providedIn: 'root'
})
export class AdminStudentService {
  private http = inject(HttpClient)
  private apiUrl = '/api/students';

  // GET ALL
  getStudents(): Observable<StudentListDto[]> {
    return this.http.get<StudentListDto[]>(this.apiUrl);
  }

  // GET BY ID
  getStudent(id: string): Observable<StudentDetailDto> {
    return this.http.get<StudentDetailDto>(`${this.apiUrl}/${id}`);
  }

  // CREATE
  createStudent(student: StudentBaseDto & { studentID: string }): Observable<any> {
    return this.http.post<any>(this.apiUrl, student);
  }

  // UPDATE
  updateStudent(id: string, student: StudentBaseDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, student);
  }

  // DELETE
  deleteStudent(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
