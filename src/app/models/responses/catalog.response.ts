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
