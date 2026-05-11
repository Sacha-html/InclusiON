export interface InstitutionResponse {
  id: number;
  encryptedId: string;
  name: string;
  address?: string;
  phone?: string;
  email?: string;
  isActive: boolean;
  createdAt: string;
}
