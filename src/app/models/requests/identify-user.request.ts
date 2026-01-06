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

export interface EmojiLoginRequest {
  userId: string;
  emojiSequence: string[];
  deviceId?: string;
  rememberDevice?: boolean;
}

export interface ColorShapeLoginRequest {
  userId: string;
  colorShapeId: number;
  deviceId?: string;
  rememberDevice?: boolean;
}

export interface TrustedDeviceLoginRequest {
  userId: string;
  deviceId: string;
  deviceToken?: string;
}

export interface ProfileSelectLoginRequest {
  userId: string;
  deviceId: string;
  requiresConfirmation?: boolean;
  confirmationPin?: string;
}
