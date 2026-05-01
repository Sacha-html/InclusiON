export interface ActivityListItemResponse {
  id: number;
  title: string;
  description?: string;
  categoryName?: string;
  skillAreaName?: string;
  templateTypeCode: string;
  templateTypeName: string;
  complexityLevel?: number;
  estimatedDurationMinutes?: number;
  isActive: boolean;
  isStandardActivity: boolean;
  createdAt: string;
}

export interface ActivityResponse {
  id: number;
  title: string;
  description?: string;
  instructions?: string;
  categoryId: number;
  categoryName?: string;
  skillAreaId?: number;
  skillAreaName?: string;
  complexityLevel?: number;
  estimatedDurationMinutes?: number;
  requiresSupervision: boolean;
  hasVisualSupport: boolean;
  hasAudioSupport: boolean;
  usesEasyReading: boolean;
  usesPictograms: boolean;
  resourcesUrl?: string;
  templateTypeId: number;
  templateTypeCode: string;
  templateTypeName: string;
  contentJson: string;
  isActive: boolean;
  isStandardActivity: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface ActivityAttemptResponse {
  id: number;
  startedAt: string;
  completedAt?: string;
  timeSpentSeconds?: number;
  result?: string;
  successPercentage?: number;
  attemptCount: number;
}

export interface ActivityAssignmentResponse {
  id: number;
  activityId: number;
  activityTitle: string;
  templateTypeCode: string;
  contentJson: string;
  personId: string;
  status: string;
  assignedAt: string;
  dueDate?: string;
  isEvaluationActivity: boolean;
  responses: ActivityAttemptResponse[];
}

// Content shape for SELECT_FIGURE template
export interface SelectFigureItem {
  id: string;
  pictogramId: number;
  label: string;
}

export interface SelectFigureContent {
  instruction: string;
  correctItemId: string;
  items: SelectFigureItem[];
}
