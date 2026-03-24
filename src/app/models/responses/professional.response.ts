export interface ProfessionalListItemResponse {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  documentNumber?: string;
  phone?: string;
  specialty?: string;
  licenseNumber?: string;
  isActive: boolean;
  email?: string;
}

export interface ProfessionalResponse {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  documentNumber?: string;
  phone?: string;
  specialty?: string;
  licenseNumber?: string;
  birthDate?: string;
  address?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  temporaryPassword?: string;
  email?: string;
}
