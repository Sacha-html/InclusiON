export interface AdminUserResponse {
  id: string;
  name: string;
  surname: string;
  fullName: string;
  email: string;
  isActive: boolean;
  createdAt: string;
  isGlobalAdmin: boolean;
  institutions: AdminInstitutionInfo[];
}

export interface AdminInstitutionInfo {
  institutionId: number;
  institutionName: string;
}

export interface CreateAdminUserResponse {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  institutionId: number;
  institutionName: string;
  temporaryPassword: string;
}
