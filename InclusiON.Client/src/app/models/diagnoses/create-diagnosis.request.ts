export interface CreateDiagnosisRequest {
  diagnosisDate: string;
  /** Asignatura / Área Pedagógica del diagnóstico. */
  primaryDiagnosis: string;
  initialObservations?: string;
  identifiedCapabilities: string;
  identifiedChallenges: string;
  requiredSupports?: string;
  pedagogicalObjectives?: string;
  recommendedStrategies?: string;
}
