export interface CreateAssignmentRequest {
  encryptedActivityId: string;
  personId: string;
  dueDate?: string;
  isEvaluationActivity: boolean;
  sequenceOrder?: number;
  bypassDuplicateWarning?: boolean;
}
