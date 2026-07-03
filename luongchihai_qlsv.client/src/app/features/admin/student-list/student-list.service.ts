import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  StudentRequest,
  StudentResponse
} from '../../../core/models/student.model';

@Injectable({
  providedIn: 'root'
})
export class AdminStudentService {
  private http = inject(HttpClient)
  private apiUrl = '/api/AdminStudents';

  // GET ALL
  getStudents(): Observable<StudentResponse[]> {
    return this.http.get<StudentResponse[]>(this.apiUrl);
  }

  // GET BY ID
  getStudent(id: string): Observable<StudentResponse> {
    return this.http.get<StudentResponse>(`${this.apiUrl}/${id}`);
  }

  // CREATE
  createStudent(student: StudentRequest): Observable<StudentResponse> {
    return this.http.post<StudentResponse>(this.apiUrl, student);
  }

  // UPDATE
  updateStudent(id: string, student: StudentRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, student);
  }

  // DELETE
  deleteStudent(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
