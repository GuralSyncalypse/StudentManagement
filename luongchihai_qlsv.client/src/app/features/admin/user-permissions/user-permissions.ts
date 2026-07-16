import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PermissionMatrixService } from '../permission-matrix/permission-matrix.service';
import { UserOption, PermissionRow } from '../../../core/models/permission.model';

interface PermissionGroup {
  groupCode: string;
  groupName: string;
  rows: PermissionRow[];
}

@Component({
  selector: 'app-user-permissions',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-permissions.html',
  styleUrl: './user-permissions.css',
})
export class UserPermissions implements OnInit {
  private permissionService = inject(PermissionMatrixService);

  users = signal<UserOption[]>([]);
  selectedUserID = signal<number>(2);
  permissionRows = signal<PermissionRow[]>([]);
  isLoading = signal<boolean>(false);
  isSaving = signal<boolean>(false);

  // Mặc định chọn 'ALL' để hiển thị tổng quan, hoặc bạn có thể đổi thành '01' nếu muốn mặc định phân hệ đầu
  selectedGroupCode = signal<string>('ALL');

  private groupNames: Record<string, string> = {
    '01': 'Quản lý tài khoản',
    '02': 'Quản lý vai trò',
    '03': 'Quản lý quyền',
    '04': 'Quản lý sinh viên',
    '05': 'Hồ sơ học thuật',
    '06': 'Quản lý khóa học',
    '07': 'Lớp học phần',
    '08': 'Đăng ký học phần',
    '09': 'Quản lý điểm',
  };

  groupedPermissions = computed<PermissionGroup[]>(() => {
    const rows = this.permissionRows();
    const groupsMap = new Map<string, PermissionRow[]>();

    rows.forEach(row => {
      const groupCode = row.permissionID.substring(0, 2);
      if (!groupsMap.has(groupCode)) {
        groupsMap.set(groupCode, []);
      }
      groupsMap.get(groupCode)!.push(row);
    });

    return Array.from(groupsMap.entries()).map(([groupCode, groupRows]) => ({
      groupCode,
      groupName: this.groupNames[groupCode] || `Phân hệ ${groupCode}`,
      rows: groupRows
    }));
  });

  // Lọc danh sách phân hệ hiển thị dựa trên ComboBox
  filteredGroupedPermissions = computed<PermissionGroup[]>(() => {
    const groups = this.groupedPermissions();
    const filterCode = this.selectedGroupCode();

    if (filterCode === 'ALL') {
      return groups;
    }
    return groups.filter(g => g.groupCode === filterCode);
  });

  ngOnInit(): void {
    this.loadMatrix();
  }

  loadMatrix(userId?: number): void {
    this.isLoading.set(true);
    const targetUserId = userId ?? this.selectedUserID();

    this.permissionService.getExclusiveMatrix(targetUserId).subscribe({
      next: (response) => {
        this.users.set(response.users);
        this.selectedUserID.set(response.selectedUserID);
        this.permissionRows.set(response.rows);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Lỗi khi tải dữ liệu quyền:', err);
        this.isLoading.set(false);
      }
    });
  }

  onUserChange(event: Event): void {
    const selectElement = event.target as HTMLSelectElement;
    const userId = Number(selectElement.value);
    this.selectedUserID.set(userId);
    this.loadMatrix(userId);
  }

  onGroupChange(event: Event): void {
    const selectElement = event.target as HTMLSelectElement;
    this.selectedGroupCode.set(selectElement.value);
  }

  toggleAssigned(targetRow: PermissionRow): void {
    this.permissionRows.update(rows =>
      rows.map(row => {
        if (row.permissionID === targetRow.permissionID) {
          const nextAssigned = !row.isAssigned;
          return {
            ...row,
            isAssigned: nextAssigned,
            isAllowed: nextAssigned ? row.isAllowed : false
          };
        }
        return row;
      })
    );
  }

  toggleAllowed(targetRow: PermissionRow): void {
    if (!targetRow.isAssigned) return;

    this.permissionRows.update(rows =>
      rows.map(row => {
        if (row.permissionID === targetRow.permissionID) {
          return {
            ...row,
            isAllowed: !row.isAllowed
          };
        }
        return row;
      })
    );
  }

  isGroupAllAssigned(groupRows: PermissionRow[]): boolean {
    return groupRows.every(r => r.isAssigned);
  }

  toggleAllInGroup(groupCode: string, currentStatus: boolean): void {
    const nextStatus = !currentStatus;
    this.permissionRows.update(rows =>
      rows.map(row => {
        if (row.permissionID.substring(0, 2) === groupCode) {
          return {
            ...row,
            isAssigned: nextStatus,
            isAllowed: nextStatus ? row.isAllowed : false
          };
        }
        return row;
      })
    );
  }

  saveChanges(): void {
    this.isSaving.set(true);
    this.permissionService.saveExclusiveMatrix(this.selectedUserID(), this.permissionRows()).subscribe({
      next: (res) => {
        alert(res.message || 'Lưu thành công!');
        this.isSaving.set(false);
        this.loadMatrix();
      },
      error: (err) => {
        console.error('Lỗi khi lưu cấu hình:', err);
        alert('Đã xảy ra lỗi hệ thống.');
        this.isSaving.set(false);
      }
    });
  }
}
