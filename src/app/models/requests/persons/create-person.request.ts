export interface CreatePersonRequest {
  firstName: string;
  lastName: string;
  documentNumber?: string;
  birthDate: string; // ISO date string

  disabilityTypeId?: number;
  photoUrl?: string;

  // Perfil Funcional
  attentionLevel?: number; // 1-5
  communicationLevel?: number; // 1-5
  usesAAC: boolean;
  usesSignLanguage: boolean;
  motorSkillLevel?: number; // 1-5

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
  loginMethodId?: number;
  pin?: string; // 4 digits
  supervisorUserId?: string;
  avatarColor?: string;
}
