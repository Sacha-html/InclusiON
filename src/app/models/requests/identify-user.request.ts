export interface IdentifyUserRequest {
  identifier: string;
  deviceId?: string;
  userType?: 'PERSON' | 'PROFESSIONAL' | 'FAMILY';
}

export interface PinLoginRequest {
  userId: number;
  pin: string;
  deviceId?: string;
  rememberDevice?: boolean;
}

export interface EmojiLoginRequest {
  userId: number;
  emojiSequence: string[];
  deviceId?: string;
  rememberDevice?: boolean;
}

export interface ColorShapeLoginRequest {
  userId: number;
  colorShapeId: number;
  deviceId?: string;
  rememberDevice?: boolean;
}

export interface TrustedDeviceLoginRequest {
  userId: number;
  deviceId: string;
  deviceToken?: string;
}

export interface ProfileSelectLoginRequest {
  userId: number;
  deviceId: string;
  requiresConfirmation?: boolean;
  confirmationPin?: string;
}
