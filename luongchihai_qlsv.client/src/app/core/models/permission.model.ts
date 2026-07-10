export interface RolePermissionRoleOption {
  roleID: number;
  roleName: string;
}

export interface RolePermissionMatrixCell {
  permissionID: number;
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
  selectedPermissionKeys: string[];
}
