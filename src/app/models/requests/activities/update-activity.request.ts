export interface UpdateActivityRequest {
  title: string;
  description?: string;
  instructions?: string;
  categoryId: number;
  skillAreaId?: number;
  complexityLevel?: number;
  estimatedDurationMinutes?: number;
  requiresSupervision: boolean;
  hasVisualSupport: boolean;
  hasAudioSupport: boolean;
  usesEasyReading: boolean;
  usesPictograms: boolean;
  resourcesUrl?: string;
  contentJson: string;
}
