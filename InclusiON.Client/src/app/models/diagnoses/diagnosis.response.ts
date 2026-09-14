export interface DiagnosisResponse {
  encryptedId: string;
  personId: string;
  professionalId: string;
  professionalName: string;
  diagnosisDate: string;
  primaryDiagnosis: string;
  initialObservations?: string;
  identifiedCapabilities?: string;
  identifiedChallenges?: string;
  requiredSupports?: string;
  pedagogicalObjectives?: string;
  recommendedStrategies?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface DiagnosisListItemResponse {
  encryptedId: string;
  diagnosisDate: string;
  primaryDiagnosis: string;
  professionalName: string;
  professionalId: string;
  createdByUserId: string;
  createdAt: string;
  isActive: boolean;
}
