import { ApiResponse } from './base/api.response';

export interface IdentifyUserData {
  userFound: boolean;
  userId?: string;
  displayName?: string;
  initial?: string;
  avatarColor?: string;
  loginMethodCode?: string;
  loginMethodName?: string;
  isTrustedDevice: boolean;
  requiresSupervision: boolean;
  userType?: 'Person' | 'Professional' | 'Family';
  errorMessage?: string;
}

export interface VisualLoginUserInfo {
  id: string;
  displayName: string;
  initial: string;
  avatarColor: string;
  userType: string;
  roles: string[];
  accessibility?: AccessibilityPreferences;
}

export interface AccessibilityPreferences {
  requiresLargeFont: boolean;
  requiresHighContrast: boolean;
  visualNoiseSensitivity: boolean;
  soundSensitivity: boolean;
}

export interface VisualLoginData {
  success: boolean;
  accessToken?: string;
  refreshToken?: string;
  expiresAt?: Date;
  user?: VisualLoginUserInfo;
  mustChangePassword?: boolean;
  errorMessage?: string;
  remainingAttempts?: number;
  isLocked: boolean;
  lockoutSecondsRemaining?: number;
}

export type IdentifyUserResponse = ApiResponse<IdentifyUserData>;
export type VisualLoginResponse = ApiResponse<VisualLoginData>;
