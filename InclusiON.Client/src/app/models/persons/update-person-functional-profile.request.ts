export interface UpdatePersonFunctionalProfileRequest {
  attentionLevel?: number;
  communicationLevel?: number;
  usesAAC: boolean;
  usesSignLanguage: boolean;
  motorSkillLevel?: number;
  interestsAndMotivators?: string;
  learningStyle?: string;
  availableResources?: string;
  additionalTherapies?: string;
  requiresLargeFont: boolean;
  requiresHighContrast: boolean;
  visualNoiseSensitivity: boolean;
  soundSensitivity: boolean;
  colorBlindnessType?: 'deuteranopia' | 'protanopia' | 'tritanopia' | null;
}
