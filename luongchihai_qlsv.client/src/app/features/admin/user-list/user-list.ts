import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, DestroyRef, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { AdminUserService } from './user-list.service';
import {
  ChangePasswordRequest,
  CreateUserRequest,
  ResetPasswordRequest,
  UpdateUserRequest,
  UserListItem
} from '../../../core/models/user.model';

type UserFormMode = 'create' | 'edit';
type NoticeType = 'success' | 'error' | '';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './user-list.html',
  styleUrl: './user-list.css',
})
export class UserList implements OnInit {
  private userService = inject(AdminUserService);
  private destroyRef = inject(DestroyRef);
  private cdr = inject(ChangeDetectorRef);

  allUsers: UserListItem[] = [];
  users: UserListItem[] = [];
  roleOptions: string[] = [];
  searchTerm = '';
  selectedStatus = 'All';
  selectedRole = 'All';
  isLoading = false;
  successMessage = '';
  errorMessage = '';
  noticeType: NoticeType = '';
  openActionMenuId: number | null = null;

  isFormOpen = false;
  formMode: UserFormMode = 'create';
  editingUserId: number | null = null;
  form = this.createEmptyForm();

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.userService.getUsers()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.allUsers = [...res];
          this.roleOptions = this.getUniqueRoles(res);
          this.applyFilters();
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: (err: any) => {
          this.showError(err.error?.message || 'Không thể tải danh sách tài khoản.');
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
  }

  onSearch(value: string): void {
    this.searchTerm = value.trim().toLowerCase();
    this.applyFilters();
  }

  onStatusChange(value: string): void {
    this.selectedStatus = value;
    this.applyFilters();
  }

  onRoleChange(value: string): void {
    this.selectedRole = value;
    this.applyFilters();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedStatus = 'All';
    this.selectedRole = 'All';
    this.applyFilters();
  }

  toggleActionMenu(userId: number): void {
    this.openActionMenuId = this.openActionMenuId === userId ? null : userId;
  }

  closeActionMenu(): void {
    this.openActionMenuId = null;
  }

  openCreateForm(): void {
    this.formMode = 'create';
    this.editingUserId = null;
    this.form = this.createEmptyForm();
    this.isFormOpen = true;
    this.clearNotice();
  }

  openEditForm(user: UserListItem): void {
    this.formMode = 'edit';
    this.editingUserId = user.userID;
    this.form = {
      username: user.username,
      email: user.email,
      phoneNumber: user.phoneNumber,
      password: '',
      roleName: user.roleNames[0] ?? 'Student',
      isActive: user.isActive
    };
    this.isFormOpen = true;
    this.clearNotice();
  }

  closeForm(): void {
    this.isFormOpen = false;
    this.editingUserId = null;
    this.form = this.createEmptyForm();
  }

  saveUser(): void {
    this.clearNotice();

    if (!this.form.username || !this.form.email || !this.form.phoneNumber || !this.form.roleName) {
      this.showError('Vui lòng nhập đầy đủ thông tin tài khoản.');
      return;
    }

    if (this.formMode === 'create' && !this.form.password) {
      this.showError('Mật khẩu khởi tạo không được để trống.');
      return;
    }

    const payload = this.normalizeForm();

    if (this.formMode === 'create') {
      this.userService.createUser(payload as CreateUserRequest)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.showSuccess('Đã tạo tài khoản mới.');
            this.closeForm();
            this.loadUsers();
          },
          error: (err: any) => {
            this.showError(err.error?.message || 'Không thể lưu tài khoản.');
          }
        });
      return;
    }

    this.userService.updateUser(this.editingUserId!, payload as UpdateUserRequest)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.showSuccess('Đã cập nhật tài khoản.');
          this.closeForm();
          this.loadUsers();
        },
        error: (err: any) => {
          this.showError(err.error?.message || 'Không thể lưu tài khoản.');
        }
      });
  }

  toggleStatus(user: UserListItem): void {
    const nextStatus = !user.isActive;
    const actionLabel = nextStatus ? 'mở khóa' : 'khóa';

    if (!confirm(`Bạn có chắc muốn ${actionLabel} tài khoản "${user.username}" không?`)) {
      return;
    }

    this.userService.updateStatus(user.userID, nextStatus)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.showSuccess(`Đã ${actionLabel} tài khoản "${user.username}".`);
          this.loadUsers();
        },
        error: (err: any) => {
          this.showError(err.error?.message || 'Không thể cập nhật trạng thái tài khoản.');
        }
      });
  }

  changePassword(user: UserListItem): void {
    const currentPassword = prompt(`Nhập mật khẩu hiện tại của tài khoản "${user.username}":`);
    if (currentPassword === null) {
      return;
    }

    const newPassword = prompt('Nhập mật khẩu mới:');
    if (newPassword === null) {
      return;
    }

    const payload: ChangePasswordRequest = {
      currentPassword,
      newPassword
    };

    this.userService.changePassword(user.userID, payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.showSuccess(`Đã đổi mật khẩu cho "${user.username}".`);
        },
        error: (err: any) => {
          this.showError(err.error?.message || 'Không thể đổi mật khẩu.');
        }
      });
  }

  resetPassword(user: UserListItem): void {
    const newPassword = prompt(`Nhập mật khẩu đặt lại cho "${user.username}":`);
    if (newPassword === null) {
      return;
    }

    const payload: ResetPasswordRequest = { newPassword };

    this.userService.resetPassword(user.userID, payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.showSuccess(`Đã đặt lại mật khẩu cho "${user.username}".`);
        },
        error: (err: any) => {
          this.showError(err.error?.message || 'Không thể đặt lại mật khẩu.');
        }
      });
  }

  deleteUser(user: UserListItem): void {
    if (!confirm(`Bạn có chắc muốn xóa tài khoản "${user.username}" không?`)) {
      return;
    }

    this.userService.deleteUser(user.userID)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.showSuccess(`Đã xóa tài khoản "${user.username}".`);
          this.closeActionMenu();
          this.loadUsers();
        },
        error: (err: any) => {
          this.showError(err.error?.message || 'Không thể xóa tài khoản.');
        }
      });
  }

  getVisibleRoleLabel(user: UserListItem): string {
    return user.roleNames.length > 0 ? user.roleNames.join(', ') : 'Chưa gán';
  }

  getFormRoleOptions(): string[] {
    return [...new Set(['Student', 'Admin', ...this.roleOptions])];
  }

  get totalCount(): number {
    return this.allUsers.length;
  }

  get visibleCount(): number {
    return this.users.length;
  }

  get activeCount(): number {
    return this.allUsers.filter(user => user.isActive).length;
  }

  get lockedCount(): number {
    return this.allUsers.filter(user => !user.isActive).length;
  }

  get appliedFiltersCount(): number {
    let count = 0;

    if (this.searchTerm) count += 1;
    if (this.selectedStatus !== 'All') count += 1;
    if (this.selectedRole !== 'All') count += 1;

    return count;
  }

  get hasFiltersApplied(): boolean {
    return this.appliedFiltersCount > 0;
  }

  private applyFilters(): void {
    const normalizedSearch = this.searchTerm;

    this.users = this.allUsers.filter(user => {
      const roleLabel = user.roleNames.join(' ').toLowerCase();
      const matchSearch = !normalizedSearch || [
        String(user.userID),
        user.username,
        user.email,
        user.phoneNumber,
        roleLabel
      ].some(value => value.toLowerCase().includes(normalizedSearch));

      const matchStatus =
        this.selectedStatus === 'All' ||
        (this.selectedStatus === 'Active' && user.isActive) ||
        (this.selectedStatus === 'Inactive' && !user.isActive);

      const matchRole =
        this.selectedRole === 'All' ||
        user.roleNames.includes(this.selectedRole);

      return matchSearch && matchStatus && matchRole;
    });

    this.cdr.detectChanges();
  }

  private getUniqueRoles(users: UserListItem[]): string[] {
    return [...new Set(users.flatMap(user => user.roleNames))]
      .filter(Boolean)
      .sort((a, b) => a.localeCompare(b, 'vi'));
  }

  private createEmptyForm(): {
    username: string;
    email: string;
    phoneNumber: string;
    password: string;
    roleName: string;
    isActive: boolean;
  } {
    return {
      username: '',
      email: '',
      phoneNumber: '',
      password: '',
      roleName: 'Student',
      isActive: true
    };
  }

  private normalizeForm(): CreateUserRequest | UpdateUserRequest {
    return {
      username: this.form.username.trim(),
      email: this.form.email.trim(),
      phoneNumber: this.form.phoneNumber.trim(),
      roleName: this.form.roleName,
      isActive: this.form.isActive,
      password: this.form.password
    } as CreateUserRequest | UpdateUserRequest;
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
    this.errorMessage = '';
    this.successMessage = '';
    this.noticeType = '';
  }
}
