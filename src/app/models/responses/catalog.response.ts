export interface CatalogItem {
  id: number;
  name: string;
  description?: string;
}

export interface AutonomyLevelItem {
  id: number;
  name: string;
  description?: string;
  requiresSupervision: boolean;
  displayOrder: number;
}

export interface ActivityCategoryItem {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface SkillAreaItem {
  id: number;
  name: string;
  description?: string;
  icon?: string;
  color?: string;
  displayOrder: number;
}

export interface ActivityTemplateTypeItem {
  id: number;
  name: string;
  code: string;
  skillAreaId?: number;
  skillAreaName?: string;
  supportsPictograms: boolean;
  supportsAudio: boolean;
}

export interface LoginMethodItem {
  id: number;
  code: string;
  name: string;
  description?: string;
  requiresPassword: boolean;
  requiresPin: boolean;
  requiresSupervisor: boolean;
  displayOrder: number;
}
