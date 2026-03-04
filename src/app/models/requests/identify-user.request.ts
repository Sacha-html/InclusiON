export interface IdentifyUserRequest {
  identifier: string;
  deviceId?: string;
  userType?: 'PERSON' | 'PROFESSIONAL' | 'FAMILY';
}

export interface PinLoginRequest {
  userId: string;
  pin: string;
  deviceId?: string;
  rememberDevice?: boolean;
}

export interface VisualStandardLoginRequest {
  userId: string;
  password: string;
  deviceId?: string;
  rememberDevice?: boolean;
}

export interface AssistedLoginRequest {
  userId: string;
  supervisorEmail: string;
  supervisorPassword: string;
  deviceId?: string;
}

export interface FamilyLoginRequest {
  userId: string;
  password: string;
  deviceId?: string;
  rememberDevice?: boolean;
}

export interface UpdateLoginMethodRequest {
  loginMethodId: number;
  pin?: string;
  supervisorUserId?: string;
}
