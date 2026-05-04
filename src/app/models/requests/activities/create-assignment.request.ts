export interface CreateAssignmentRequest {
  activityId: number;
  personId: string;
  dueDate?: string;
  isEvaluationActivity: boolean;
  sequenceOrder?: number;
}
