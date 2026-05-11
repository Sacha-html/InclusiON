export interface ProfessionalPersonResponse {
  professionalId: string;
  personId: string;
  personFirstName: string;
  personLastName: string;
  personFullName: string;
  avatarColor?: string;
  disabilityTypeName?: string;
  age?: number;
  isPrimaryProfessional: boolean;
  canSuperviseLogin: boolean;
  isActive: boolean;
  assignedAt: string;
}

export interface ProfessionalInstitutionResponse {
  professionalId: string;
  institutionId: string;
  institutionName: string;
  isActive: boolean;
  assignedAt: string;
}
