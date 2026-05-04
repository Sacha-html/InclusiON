export interface RoadmapResponse {
  id: number;
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
  skillAreaId: number;
  skillAreaName: string;
  color?: string | null;
  icon?: string | null;
  displayOrder: number;
  activities: RoadmapActivityResponse[];
}

export interface RoadmapActivityResponse {
  id: number;
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
