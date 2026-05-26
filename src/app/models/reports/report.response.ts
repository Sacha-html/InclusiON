export enum ReportStatus {
  Draft     = 0,
  Submitted = 1,
  Approved  = 2,
  Rejected  = 3,
}

export interface ReportListItemResponse {
  id: number;
  encryptedId: string;
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
  isReadByFamily?: boolean;
}

export interface ReportResponse extends ReportListItemResponse {
  content: string;
  achievedGoals?: string;
  areasToReinforce?: string;
  futureRecommendations?: string;
  nextObjectives?: string;
}
