export type ReportStatus = 'Draft' | 'Submitted' | 'Approved' | 'Rejected';

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
  status: ReportStatus;
  adminComment?: string;
  approvedAt?: string;
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