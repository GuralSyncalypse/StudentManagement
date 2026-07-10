import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  RolePermissionMatrixResponse,
  SaveRolePermissionMatrixRequest
} from '../../../core/models/permission.model';

@Injectable({
  providedIn: 'root'
})
export class PermissionMatrixService {
  private http = inject(HttpClient);
  private apiUrl = '/api/rolepermissions';

  getMatrix(roleId?: number): Observable<RolePermissionMatrixResponse> {
    let params = new HttpParams();
    if (typeof roleId === 'number') {
      params = params.set('roleId', String(roleId));
    }

    return this.http.get<RolePermissionMatrixResponse>(this.apiUrl, { params });
  }

  saveMatrix(roleId: number, payload: SaveRolePermissionMatrixRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${roleId}`, payload);
  }
}
