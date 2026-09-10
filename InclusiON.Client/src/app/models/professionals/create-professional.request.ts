export interface CreateProfessionalRequest {
  firstName: string;
  lastName: string;
  email: string;
  documentNumber?: string;
  phone?: string;
  specialty?: string;
  specialtyId?: number;
  licenseNumber?: string;
  birthDate?: string;
}
