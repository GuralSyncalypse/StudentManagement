import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  ChangePasswordRequest,
  CreateUserRequest,
  ResetPasswordRequest,
  UpdateUserRequest,
  UserDetail,
  UserListItem,
  UserQuery
} from '../../../core/models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AdminUserService {
  private http = inject(HttpClient);
  private apiUrl = '/api/users';

  getUsers(query?: UserQuery): Observable<UserListItem[]> {
    let params = new HttpParams();

    if (query?.search) {
      params = params.set('search', query.search);
    }

    if (query?.isActive !== null && query?.isActive !== undefined) {
      params = params.set('isActive', String(query.isActive));
    }

    if (query?.role && query.role !== 'All') {
      params = params.set('role', query.role);
    }

    return this.http.get<UserListItem[]>(this.apiUrl, { params });
  }

  getUser(id: number): Observable<UserDetail> {
    return this.http.get<UserDetail>(`${this.apiUrl}/${id}`);
  }

  createUser(user: CreateUserRequest): Observable<UserDetail> {
    return this.http.post<UserDetail>(this.apiUrl, user);
  }

  updateUser(id: number, user: UpdateUserRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, user);
  }

  deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  updateStatus(id: number, isActive: boolean): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/status`, { isActive });
  }

  changePassword(id: number, payload: ChangePasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/change-password`, payload);
  }

  resetPassword(id: number, payload: ResetPasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/reset-password`, payload);
  }
}
