export interface RegisterProfessionalRequest {
  firstName: string;
  lastName: string;
  email: string;
  documentNumber?: string;
  phone?: string;
  specialty: string;
  licenseNumber?: string;
  birthDate?: string;
  institutionId?: number;
}