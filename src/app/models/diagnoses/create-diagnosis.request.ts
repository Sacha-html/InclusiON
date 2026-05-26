export interface CreateDiagnosisRequest {
  diagnosisDate: string;
  primaryDiagnosis: string;
  initialObservations?: string;
  identifiedCapabilities?: string;
  identifiedChallenges?: string;
  requiredSupports?: string;
  pedagogicalObjectives?: string;
  recommendedStrategies?: string;
}
