export interface CreateProfessionalRequest {
  firstName: string;
  lastName: string;
  email: string;
  documentNumber?: string;
  phone?: string;
  speciality?: string;
  licenseNumber?: string;
  birthDate?: string;
  address?: string;
}
