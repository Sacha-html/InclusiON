export interface DiagnosisResponse {
  id: number;
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
  diagnosisDate: string;
  primaryDiagnosis: string;
  professionalName: string;
  professionalId: string;
  createdByUserId: string;
  createdAt: string;
}
