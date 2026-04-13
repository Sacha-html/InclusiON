export interface ReportListItemResponse {
  id: number;
  personId: string;
  personName: string;
  professionalId: string;
  professionalName: string;
  reportTypeId: number;
  reportTypeName: string;
  title: string;
  reportDate: string;
  periodStartDate?: string;
  periodEndDate?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface ReportResponse extends ReportListItemResponse {
  content: string;
  achievedGoals?: string;
  areasToReinforce?: string;
  futureRecommendations?: string;
  nextObjectives?: string;
}