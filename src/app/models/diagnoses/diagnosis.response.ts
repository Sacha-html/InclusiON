export interface DiagnosisResponse {
  id: number;
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
  id: number;
  encryptedId: string;
  diagnosisDate: string;
  primaryDiagnosis: string;
  professionalName: string;
  professionalId: string;
  createdByUserId: string;
  createdAt: string;
}
