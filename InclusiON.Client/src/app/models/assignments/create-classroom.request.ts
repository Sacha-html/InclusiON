export interface CreateClassroomRequest {
  name: string;
  personIds: string[];
  isPrimaryProfessional?: boolean;
  canSuperviseLogin?: boolean;
}
