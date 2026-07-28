export interface LoginMethod {
  id: number;
  code: string;
  name: string;
  description: string;
  requiresPassword: boolean;
  requiresPin: boolean;
  requiresSupervisor: boolean;
  displayOrder: number;
}

export interface LoginMethodsResponse {
  success: boolean;
  message: string;
  data: LoginMethod[];
  errors: string[];
}
