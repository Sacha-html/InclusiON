export interface LoginRequest {
  email?: string;
  password?: string;
  rememberMe?: boolean;
  allowedRoles?: string[];
}
