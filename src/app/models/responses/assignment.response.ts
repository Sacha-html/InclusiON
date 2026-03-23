export interface ProfessionalPersonResponse {
  professionalId: string;
  personId: string;
  personFirstName: string;
  personLastName: string;
  personFullName: string;
  isPrimaryProfessional: boolean;
  canSuperviseLogin: boolean;
  isActive: boolean;
  assignedAt: string;
}

export interface ProfessionalInstitutionResponse {
  professionalId: string;
  institutionId: number;
  institutionName: string;
  isActive: boolean;
  assignedAt: string;
}
