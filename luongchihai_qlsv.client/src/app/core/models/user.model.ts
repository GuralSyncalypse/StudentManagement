export interface UserListItem {
  userID: number;
  username: string;
  email: string;
  phoneNumber: string;
  isActive: boolean;
  createdAt: string;
  roleNames: string[];
}

export interface UserDetail extends UserListItem {
}

export interface UserQuery {
  search?: string;
  isActive?: boolean | null;
  role?: string;
}

export interface CreateUserRequest {
  username: string;
  email: string;
  phoneNumber: string;
  password: string;
  roleName: string;
  isActive: boolean;
}

export interface UpdateUserRequest {
  username: string;
  email: string;
  phoneNumber: string;
  roleName: string;
  isActive: boolean;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface ResetPasswordRequest {
  newPassword: string;
}
