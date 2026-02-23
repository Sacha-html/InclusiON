export interface UserProfileResponse {
  id: string;
  name: string;
  surname: string;
  fullName: string;
  email: string;
  phone?: string;
  role: string;
  isActive: boolean;
  activeSessionsCount: number;
  permissions: string[];
  emailConfirmed: boolean;
  phoneNumberConfirmed: boolean;
}
