export interface User {
  id: string;
  email: string;
  name: string;
  surname: string;
  displayName?: string;
  role: string;
  isActive: boolean;
  createdAt: Date;
  lastLogin?: Date;
}
