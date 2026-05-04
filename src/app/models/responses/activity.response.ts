export enum ActivityAssignmentStatus {
  Pendiente  = 'Pendiente',
  EnProgreso = 'EnProgreso',
  Completada = 'Completada',
  Cancelada  = 'Cancelada',
}

export enum ActivityResponseResult {
  Exito   = 'Exito',
  Parcial = 'Parcial',
  Fallido = 'Fallido',
}

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
  result?: ActivityResponseResult;
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
  status: ActivityAssignmentStatus;
  assignedAt: string;
  dueDate?: string;
  isEvaluationActivity: boolean;
  responses: ActivityAttemptResponse[];
}

// Content shapes — re-exportados desde player.models para retrocompatibilidad
export type {
  SelectFigureItem,
  SelectFigureContent,
  OrderSequenceItem,
  OrderSequenceContent,
  MatchPair,
  MatchImageWordContent,
  VisualSumOption,
  VisualSumContent,
  CompleteLetterContent,
} from '../../views/aac/activities/player/player.models';
