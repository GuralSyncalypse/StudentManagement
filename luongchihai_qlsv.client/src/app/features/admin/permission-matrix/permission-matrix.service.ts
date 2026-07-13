import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RolePermissionMatrixResponse, SaveRolePermissionMatrixRequest } from '../../../core/models/permission.model';

@Injectable({
  providedIn: 'root'
})
export class PermissionMatrixService {
  private http = inject(HttpClient);
  private apiUrl = 'api/rolepermissions'; // Thay bằng URL API thực tế của bạn

  getMatrix(roleId?: number): Observable<RolePermissionMatrixResponse> {
    const url = roleId ? `${this.apiUrl}?roleId=${roleId}` : this.apiUrl;
    return this.http.get<RolePermissionMatrixResponse>(url);
  }

  // Tham số thứ 2 sử dụng SaveRolePermissionMatrixRequest chứa mảng selectedPermissionIds
  saveMatrix(roleId: number, data: SaveRolePermissionMatrixRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${roleId}`, data);
  }
}
