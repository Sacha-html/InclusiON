export interface CreateProfessionalRequest {
  firstName: string;
  lastName: string;
  email: string;
  documentNumber?: string;
  phone?: string;
  specialty?: string;
  licenseNumber?: string;
  birthDate?: string;
}
