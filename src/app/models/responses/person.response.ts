export interface PersonResponse {
  id: string;
  userId: string;

  firstName: string;
  lastName: string;
  fullName: string;
  documentNumber?: string;
  birthDate: string;
  age: number;
  photoUrl?: string;

  // Perfil Funcional
  attentionLevel?: number;
  communicationLevel?: number;
  usesAAC: boolean;
  usesSignLanguage: boolean;
  motorSkillLevel?: number;

  // Preferencias
  interestsAndMotivators?: string;
  learningStyle?: string;
  availableResources?: string;
  additionalTherapies?: string;

  // Accesibilidad
  requiresLargeFont: boolean;
  requiresHighContrast: boolean;
  visualNoiseSensitivity: boolean;
  soundSensitivity: boolean;

  // Configuracion de Acceso
  autonomyLevelId?: number;
  autonomyLevelName?: string;
  loginMethodId?: number;
  loginMethodName?: string;
  hasPinConfigured: boolean;
  supervisorUserId?: string;
  supervisorName?: string;
  avatarColor?: string;

  // Tipo de Discapacidad
  disabilityTypeId?: number;
  disabilityTypeName?: string;

  // Estado
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface PersonListItemResponse {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  documentNumber?: string;
  birthDate: string;
  age: number;
  photoUrl?: string;
  avatarColor?: string;

  disabilityTypeId?: number;
  disabilityTypeName?: string;

  autonomyLevelId?: number;
  autonomyLevelName?: string;

  loginMethodName?: string;

  isActive: boolean;
  representativeNames?: string;
}
