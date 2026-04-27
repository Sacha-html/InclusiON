export interface UpdatePersonRequest {
  firstName?: string;
  lastName?: string;
  documentNumber?: string;
  birthDate?: string; // ISO date string

  disabilityTypeId?: number;
  photoUrl?: string;

  // Perfil Funcional
  attentionLevel?: number; // 1-5
  communicationLevel?: number; // 1-5
  usesAAC?: boolean;
  usesSignLanguage?: boolean;
  motorSkillLevel?: number; // 1-5

  // Preferencias
  interestsAndMotivators?: string;
  learningStyle?: string;
  availableResources?: string;
  additionalTherapies?: string;

  // Accesibilidad
  requiresLargeFont?: boolean;
  requiresHighContrast?: boolean;
  visualNoiseSensitivity?: boolean;
  soundSensitivity?: boolean;
  colorBlindnessType?: 'deuteranopia' | 'protanopia' | 'tritanopia' | null;

  // Configuracion de Acceso
  autonomyLevelId?: number;
  supervisorUserId?: string;
  avatarColor?: string;
}
