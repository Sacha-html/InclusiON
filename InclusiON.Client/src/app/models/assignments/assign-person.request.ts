export interface AssignPersonRequest {
  personId: string;
  isPrimaryProfessional?: boolean;
  canSuperviseLogin?: boolean;
  classroomId?: string | null;
}
