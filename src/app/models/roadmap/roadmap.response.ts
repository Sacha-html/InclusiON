export interface RoadmapResponse {
  id: number;
  encryptedId: string;
  personId: string;
  createdByProfessionalId: string;
  createdByProfessionalFullName: string;
  notes?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  areas: RoadmapAreaResponse[];
}

export interface RoadmapAreaResponse {
  id: number;
  encryptedId: string;
  skillAreaId: number;
  skillAreaName: string;
  color?: string | null;
  icon?: string | null;
  displayOrder: number;
  activities: RoadmapActivityResponse[];
}

export interface RoadmapActivityResponse {
  id: number;
  encryptedId: string;
  activityId: number;
  activityTitle: string;
  sequenceOrder: number;
  isUnlocked: boolean;
  unlockedAt?: string | null;
  unlockThresholdPercent: number;
  timeLimitSeconds?: number | null;
  maxAttempts?: number | null;
  showHints: boolean;
  difficultyLevel: number;
}

export interface AdaptiveAdjustmentLogResponse {
  id: number;
  adjustmentType: string;
  previousValue: string;
  newValue: string;
  reason: string;
  adjustedAt: string;
}

export interface AdaptiveEngineConfigResponse {
  id: number;
  personRoadmapActivityId: number;
  isEnabled: boolean;
  minDifficultyLevel: number;
  maxDifficultyLevel: number;
  minTimeLimitSeconds?: number | null;
  maxTimeLimitSeconds?: number | null;
  consecutiveSuccessToUpgrade: number;
  consecutiveFailuresToDowngrade: number;
  successThresholdPercent: number;
  frustrationThreshold: number;
}

export interface SkillRadarPointResponse {
  areaName: string;
  color?: string | null;
  icon?: string | null;
  avgSuccessPercent?: number | null;
  totalResponses: number;
}
