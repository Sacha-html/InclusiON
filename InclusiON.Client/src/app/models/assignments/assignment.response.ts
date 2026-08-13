export interface ProfessionalPersonResponse {
  professionalId: string;
  personId: string;
  personFirstName: string;
  personLastName: string;
  personFullName: string;
  personDocumentNumber?: string;
  avatarColor?: string;
  disabilityTypeName?: string;
  age?: number;
  isPrimaryProfessional: boolean;
  canSuperviseLogin: boolean;
  isActive: boolean;
  assignedAt: string;
  classroomId?: string;
  classroomName?: string;
}

export interface ClassroomResponse {
  id: string;
  name: string;
  professionalId: string;
  isActive: boolean;
  studentCount: number;
}

export interface ProfessionalInstitutionResponse {
  professionalId: string;
  institutionId: string;
  institutionName: string;
  isActive: boolean;
  assignedAt: string;
}
