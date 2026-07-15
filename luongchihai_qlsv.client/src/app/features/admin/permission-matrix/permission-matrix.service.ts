import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RolePermissionMatrixResponse, SaveRolePermissionMatrixRequest, UserPermissionMatrixResponse, PermissionRow } from '../../../core/models/permission.model';

@Injectable({
  providedIn: 'root'
})
export class PermissionMatrixService {
  private http = inject(HttpClient);
  private apiUrl = 'api/rolepermissions';

  getMatrix(roleId?: number): Observable<RolePermissionMatrixResponse> {
    const url = roleId ? `${this.apiUrl}?roleId=${roleId}` : this.apiUrl;
    return this.http.get<RolePermissionMatrixResponse>(url);
  }

  // Tham số thứ 2 sử dụng SaveRolePermissionMatrixRequest chứa mảng selectedPermissionIds
  saveMatrix(roleId: number, data: SaveRolePermissionMatrixRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${roleId}`, data);
  }

  getExclusiveMatrix(userId?: number): Observable<UserPermissionMatrixResponse> {
    const url = userId ? `${this.apiUrl}/exclusive?userId=${userId}` : this.apiUrl;
    return this.http.get<UserPermissionMatrixResponse>(url);
  }

  saveExclusiveMatrix(userId: number, rows: PermissionRow[]): Observable<any> {
    // Chỉ lấy thông tin tối giản cần thiết để gửi lên Server
    const payload = {
      userID: userId,
      permissions: rows.map(row => ({
        permissionID: row.permissionID,
        isAssigned: row.isAssigned,
        isAllowed: row.isAllowed
      }))
    };
    return this.http.post(`${this.apiUrl}/save-permissions`, payload);
  }
}
