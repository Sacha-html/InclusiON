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
  encryptedId: string;
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
  isTemplate: boolean;
  createdAt: string;
  authorName?: string;
  roadmapOrder?: number;
}

export interface ActivityResponse {
  id: number;
  encryptedId: string;
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
  isTemplate: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface ActivityAttemptResponse {
  id: number;
  encryptedId: string;
  startedAt: string;
  completedAt?: string;
  timeSpentSeconds?: number;
  result?: ActivityResponseResult;
  successPercentage?: number;
  attemptCount: number;
  requiredSupport: boolean;
  frustrationLevel?: number;
}

export interface ActivityAssignmentResponse {
  id: number;
  encryptedId: string;
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
