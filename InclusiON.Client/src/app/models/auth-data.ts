import { User } from "./user";
import { AccessibilityPreferences } from "./responses/visual-login.response";

export interface AuthData {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
  mustChangePassword?: boolean;
  accessibility?: AccessibilityPreferences;
}
