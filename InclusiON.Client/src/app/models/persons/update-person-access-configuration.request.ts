export interface UpdatePersonAccessConfigurationRequest {
  autonomyLevelId: number;
  loginMethodId: number;
  avatarColor: string;
  pin?: string;
  supervisorUserId?: string;
}
