import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, DestroyRef, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PermissionMatrixService } from './permission-matrix.service';
import {
  RolePermissionMatrixCell,
  RolePermissionMatrixResponse,
  RolePermissionMatrixRow,
  RolePermissionRoleOption
} from '../../../core/models/permission.model';

type NoticeType = 'success' | 'error' | '';

@Component({
  selector: 'app-permission-matrix',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './permission-matrix.html',
  styleUrl: './permission-matrix.css'
})
export class PermissionMatrixComponent implements OnInit {
  private permissionService = inject(PermissionMatrixService);
  private destroyRef = inject(DestroyRef);
  private cdr = inject(ChangeDetectorRef);

  roles: RolePermissionRoleOption[] = [];
  selectedRoleId: number | null = null;
  rows: RolePermissionMatrixRow[] = [];
  filteredRows: RolePermissionMatrixRow[] = [];
  searchTerm = '';
  isLoading = false;
  isSaving = false;
  successMessage = '';
  errorMessage = '';
  noticeType: NoticeType = '';

  readonly crudColumns = ['create', 'read', 'update', 'delete'];

  ngOnInit(): void {
    this.loadMatrix();
  }

  loadMatrix(roleId?: number): void {
    this.isLoading = true;
    this.clearNotice();

    this.permissionService.getMatrix(roleId ?? undefined)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.applyResponse(response);
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: (err: any) => {
          this.isLoading = false;
          this.showError(err.error?.message || 'Không thể tải ma trận phân quyền.');
        }
      });
  }

  onRoleChange(value: string): void {
    const nextRoleId = Number(value);
    if (!Number.isFinite(nextRoleId)) {
      return;
    }

    this.selectedRoleId = nextRoleId;
    this.loadMatrix(nextRoleId);
  }

  onSearch(value: string): void {
    this.searchTerm = value.trim().toLowerCase();
    this.applyFilters();
  }

  toggleCell(cell: RolePermissionMatrixCell, checked: boolean): void {
    cell.isAssigned = checked;
    this.clearNotice();
  }

  // === ĐÃ SỬA: Hàm save chuyển sang gom gom permissionId thay vì permissionKey ===
  save(): void {
    if (this.selectedRoleId === null) {
      return;
    }

    this.isSaving = true;
    this.clearNotice();

    const selectedPermissionIds = this.rows
      .flatMap(row => row.cells)
      .filter(cell => cell.isAssigned)
      .map(cell => cell.permissionId); // Lấy mã số ID dạng '0101', '0102'

    // Gửi payload dạng { selectedPermissionIds: [...] } phù hợp DTO Backend mới
    this.permissionService.saveMatrix(this.selectedRoleId, { selectedPermissionIds })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isSaving = false;
          this.showSuccess('Đã cập nhật quyền cho role đã chọn.');
          this.loadMatrix(this.selectedRoleId!);
        },
        error: (err: any) => {
          this.isSaving = false;
          this.showError(err.error?.message || 'Không thể lưu phân quyền.');
        }
      });
  }

  reload(): void {
    this.loadMatrix(this.selectedRoleId ?? undefined);
  }

  get totalPermissions(): number {
    return this.rows.reduce((sum, row) => sum + row.cells.length, 0);
  }

  get assignedCount(): number {
    return this.rows
      .flatMap(row => row.cells)
      .filter(cell => cell.isAssigned)
      .length;
  }

  get hasRows(): boolean {
    return this.filteredRows.length > 0;
  }

  // Lưu ý: Check kỹ trường roleID (Backend trả về Pascal/camelCase ra sao thì sửa lại dòng này)
  get selectedRole(): RolePermissionRoleOption | undefined {
    return this.roles.find(role => (role as any).roleID === this.selectedRoleId || (role as any).roleId === this.selectedRoleId);
  }

  getCell(row: RolePermissionMatrixRow, action: string): RolePermissionMatrixCell | null {
    const normalizedAction = this.normalize(action);
    return row.cells.find(cell => this.normalize(cell.action) === normalizedAction) ?? null;
  }

  getActionLabel(action: string): string {
    return action.toUpperCase();
  }

  private applyResponse(response: RolePermissionMatrixResponse): void {
    this.roles = response.roles ?? [];

    const firstRole = this.roles[0] as any;
    this.selectedRoleId = response.selectedRoleID || firstRole?.roleID || firstRole?.roleId || null;

    this.rows = (response.rows ?? []).map(row => ({
      ...row,
      cells: row.cells.map(cell => {
        const rawCell = cell as any;
        return {
          ...cell,
          // SỬA TẠI ĐÂY: Ép nhận diện cả 3 kiểu đặt tên phổ biến từ Backend đổ về
          permissionId: rawCell.permissionId || rawCell.permissionID || rawCell.PermissionID,
          isAssigned: cell.isAssigned
        };
      })
    }));
    this.applyFilters();
  }

  private applyFilters(): void {
    const normalized = this.searchTerm;

    if (!normalized) {
      this.filteredRows = [...this.rows];
      return;
    }

    this.filteredRows = this.rows
      .map(row => {
        const rowMatches = [
          row.resource,
          row.resourceLabel,
          // Bổ sung thêm cell.permissionId vào chuỗi tìm kiếm để hỗ trợ tìm theo mã số (VD: gõ "0101")
          ...row.cells.map(cell => `${cell.permissionId} ${cell.permissionKey} ${cell.action} ${cell.description}`)
        ].some(value => value.toLowerCase().includes(normalized));

        if (rowMatches) {
          return row;
        }

        return {
          ...row,
          cells: row.cells.filter(cell =>
            `${cell.permissionId} ${cell.permissionKey} ${cell.action} ${cell.description}`.toLowerCase().includes(normalized)
          )
        };
      })
      .filter(row => row.cells.length > 0 || [
        row.resource,
        row.resourceLabel
      ].some(value => value.toLowerCase().includes(normalized)));
  }

  private normalize(value: string): string {
    return value.trim().toLowerCase();
  }

  private showSuccess(message: string): void {
    this.successMessage = message;
    this.errorMessage = '';
    this.noticeType = 'success';
    this.cdr.detectChanges();
  }

  private showError(message: string): void {
    this.errorMessage = message;
    this.successMessage = '';
    this.noticeType = 'error';
    this.cdr.detectChanges();
  }

  private clearNotice(): void {
    this.successMessage = '';
    this.errorMessage = '';
    this.noticeType = '';
  }
}
