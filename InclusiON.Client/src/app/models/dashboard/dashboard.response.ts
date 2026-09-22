// ── Family Dashboard ─────────────────────────────────────────────────────────

export interface RecentActivityResultResponse {
  assignmentId: number;
  activityTitle: string;
  result: string;
  successPercentage: number;
  completedAt: string;
}

export interface RoadmapLevelStepResponse {
  level: number;
  title: string;
  status: 'completed' | 'struggling' | 'active' | 'locked';
}

export interface FamilyPersonSummaryResponse {
  personId: string;
  fullName: string;
  avatarColor: string;
  recentActivities: RecentActivityResultResponse[];
  approvedReportsCount: number;
  latestReportTitle?: string;
  latestReportDate?: string;
  currentRoadmapLevel: number;
  roadmapLevelName: string;
  gasStatusLabel: string;
  averageSuccessRate: number;
  hasFrustrationAlert: boolean;
  roadmapLevels?: RoadmapLevelStepResponse[];
}

export interface FamilyDashboardResponse {
  persons: FamilyPersonSummaryResponse[];
  unreadMessages: number;
}

// ── Admin Dashboard ──────────────────────────────────────────────────────────

export interface AdminDashboardResponse {
  totalProfessionals: number;
  pendingValidations: number;
  totalFamilies: number;
  totalPersons: number;
  /** Only present for GlobalAdmin */
  totalInstitutions?: number;
  activeAssignments: number;
  reportsPendingApproval: number;
  reportsApprovedThisMonth: number;
}
