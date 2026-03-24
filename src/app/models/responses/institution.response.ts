export interface InstitutionResponse {
  id: number;
  name: string;
  address?: string;
  phone?: string;
  email?: string;
  isActive: boolean;
  createdAt: string;
}
