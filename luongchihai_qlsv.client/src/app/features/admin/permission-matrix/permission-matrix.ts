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

  // --- CẢI TIẾN THAO TÁC HÀNG LOẠT (BULK ACTIONS) ---

  /** Bật/Tắt tất cả các ô trong một hàng cụ thể */
  toggleRow(row: RolePermissionMatrixRow, checked: boolean): void {
    row.cells.forEach(cell => {
      cell.isAssigned = checked;
    });
    this.clearNotice();
  }

  /** Kiểm tra xem toàn bộ các quyền trong hàng đã được chọn chưa */
  isRowAllChecked(row: RolePermissionMatrixRow): boolean {
    if (!row.cells.length) return false;
    return row.cells.every(cell => cell.isAssigned);
  }

  /** Bật/Tắt tất cả các ô thuộc một cột (Create/Read/Update/Delete) trên tất cả các hàng đang hiển thị */
  toggleColumn(action: string, checked: boolean): void {
    const normalizedAction = this.normalize(action);
    this.filteredRows.forEach(row => {
      const cell = row.cells.find(c => this.normalize(c.action) === normalizedAction);
      if (cell) {
        cell.isAssigned = checked;
      }
    });
    this.clearNotice();
  }

  /** Kiểm tra xem toàn bộ cột đó đã được tích chọn hết chưa */
  isColumnAllChecked(action: string): boolean {
    if (!this.filteredRows.length) return false;
    const normalizedAction = this.normalize(action);

    // Chỉ kiểm tra các ô thực tế tồn tại trên cột đó ở các hàng đang hiển thị
    const targetCells = this.filteredRows
      .map(row => row.cells.find(c => this.normalize(c.action) === normalizedAction))
      .filter((cell): cell is RolePermissionMatrixCell => !!cell);

    if (!targetCells.length) return false;
    return targetCells.every(cell => cell.isAssigned);
  }

  // --------------------------------------------------

  save(): void {
    if (this.selectedRoleId === null) {
      return;
    }

    this.isSaving = true;
    this.clearNotice();

    const selectedPermissionIds = this.rows
      .flatMap(row => row.cells)
      .filter(cell => cell.isAssigned)
      .map(cell => cell.permissionId);

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
