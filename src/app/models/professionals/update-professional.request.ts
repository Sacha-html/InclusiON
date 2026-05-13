export interface UpdateProfessionalRequest {
  firstName?: string;
  lastName?: string;
  documentNumber?: string;
  phone?: string;
  specialty?: string;
  licenseNumber?: string;
  birthDate?: string; // ISO date string
}
