export interface CreateReportRequest {
  personId: string;
  title: string;
  content: string;
  reportTypeId: number;
  reportDate: string;
  periodStartDate?: string;
  periodEndDate?: string;
  achievedGoals?: string;
  areasToReinforce?: string;
  futureRecommendations?: string;
  nextObjectives?: string;
}