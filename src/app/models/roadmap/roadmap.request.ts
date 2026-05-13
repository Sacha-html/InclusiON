export interface CreateRoadmapRequest {
  notes?: string | null;
}

export interface UpdateRoadmapNotesRequest {
  notes?: string | null;
}

export interface AddRoadmapAreaRequest {
  skillAreaId: number;
  displayOrder: number;
}

export interface AddRoadmapActivityRequest {
  activityId: number;
  sequenceOrder: number;
  unlockThresholdPercent: number;
  timeLimitSeconds?: number | null;
  maxAttempts?: number | null;
  showHints: boolean;
  difficultyLevel: number;
}

export interface ReorderActivityItem {
  id: number;
  sequenceOrder: number;
}

export interface ReorderRoadmapActivitiesRequest {
  activities: ReorderActivityItem[];
}
