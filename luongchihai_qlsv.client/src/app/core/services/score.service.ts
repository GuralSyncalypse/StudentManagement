import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { Score, Enrollment } from '../models/enrollment.model';

@Injectable({
  providedIn: 'root'
})
export class ScoreService {
  private http = inject(HttpClient)
  private apiUrl = '/api/Scores';

  // GET ALL
  getScores(): Observable<Score[]> {
    return this.http.get<Score[]>(this.apiUrl);
  }

  // GET BY ID
  getScore(id: number): Observable<Score> {
    return this.http.get<Score>(`${this.apiUrl}/${id}`);
  }

  // CREATE
  saveScores(scores: Score[]): Observable<Score[]> {
    return this.http.post<Score[]>(this.apiUrl, scores);
  }

  // UPDATE
  updateStudent(id: number, score: Score): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, score);
  }

  // DELETE
  deleteStudent(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
