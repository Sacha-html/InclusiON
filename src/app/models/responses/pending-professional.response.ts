export interface PendingProfessionalResponse {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  documentNumber?: string;
  phone?: string;
  email?: string;
  specialty?: string;
  licenseNumber?: string;
  isActive: boolean;
  createdAt: string;
}