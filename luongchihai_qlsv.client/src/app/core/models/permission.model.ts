export interface RolePermissionRoleOption {
  roleID: number; // Hoặc roleId tùy cấu hình JSON serializer của bạn
  roleName: string;
}

export interface RolePermissionMatrixCell {
  permissionId: string;
  permissionKey: string;
  action: string;
  description: string;
  isAssigned: boolean;
}

export interface RolePermissionMatrixRow {
  resource: string;
  resourceLabel: string;
  permissionCount: number;
  cells: RolePermissionMatrixCell[];
}

export interface RolePermissionMatrixResponse {
  roles: RolePermissionRoleOption[];
  selectedRoleID: number;
  rows: RolePermissionMatrixRow[];
}

export interface SaveRolePermissionMatrixRequest {
  selectedPermissionIds: string[];
}


export interface UserOption {
  userID: number;
  username: string;
}

export interface PermissionRow {
  permissionID: string;
  description: string;
  isAssigned: boolean;
  isAllowed: boolean;
}

export interface UserPermissionMatrixResponse {
  users: UserOption[];
  selectedUserID: number;
  rows: PermissionRow[];
}
