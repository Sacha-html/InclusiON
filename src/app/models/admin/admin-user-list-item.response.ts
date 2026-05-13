export interface AdminUserListItemResponse {
  userId: string;
  email: string;
  fullName: string;
  role: string;
  isActive: boolean;
  lastLoginDate: string | null;
  createdAt: string;
  mustChangePassword: boolean;
}
