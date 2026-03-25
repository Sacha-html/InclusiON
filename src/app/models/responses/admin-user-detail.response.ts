export interface AdminUserDetailResponse {
  userId: string;
  email: string;
  name: string | null;
  surname: string | null;
  fullName: string;
  role: string;
  isActive: boolean;
  lastLoginDate: string | null;
  lastLoginIpAddress: string | null;
  createdAt: string;
  mustChangePassword: boolean;
  linkedEntity: LinkedEntityInfo | null;
}

export interface LinkedEntityInfo {
  entityType: string;
  entityId: string | null;
  specialty: string | null;
  licenseNumber: string | null;
  documentNumber: string | null;
  phone: string | null;
  relationship: string | null;
}

export interface ResetPasswordResultResponse {
  temporaryPassword: string;
  userEmail: string;
}
