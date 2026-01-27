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

/** @deprecated Use PinLoginRequest or AssistedLoginRequest instead */
export interface EmojiLoginRequest {
  userId: string;
  emojiSequence: string[];
  deviceId?: string;
  rememberDevice?: boolean;
}

/** @deprecated Use PinLoginRequest or AssistedLoginRequest instead */
export interface ColorShapeLoginRequest {
  userId: string;
  colorShapeId: number;
  deviceId?: string;
  rememberDevice?: boolean;
}

/** @deprecated Use PinLoginRequest or AssistedLoginRequest instead */
export interface TrustedDeviceLoginRequest {
  userId: string;
  deviceId: string;
  deviceToken?: string;
}

/** @deprecated Use PinLoginRequest or AssistedLoginRequest instead */
export interface ProfileSelectLoginRequest {
  userId: string;
  deviceId: string;
  requiresConfirmation?: boolean;
  confirmationPin?: string;
}

export interface UpdateLoginMethodRequest {
  loginMethodId: number;
  pin?: string;
  supervisorUserId?: string;
}
