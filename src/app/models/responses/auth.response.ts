import { AuthData } from "../auth-data";

export interface AuthResponse {
  success: boolean;
  message: string;
  data: AuthData;
  errors: string[];
  timestamp: string;
}