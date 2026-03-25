export interface FamilyResponse {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  documentNumber?: string;
  phone?: string;
  relationship?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  temporaryPassword?: string;
  email?: string;
  linkedPersons?: LinkedPersonInfo[];
}

export interface FamilyListItemResponse {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  documentNumber?: string;
  phone?: string;
  relationship?: string;
  isActive: boolean;
  email?: string;
  linkedPersons: LinkedPersonInfo[];
}

export interface LinkedPersonInfo {
  personId: string;
  fullName: string;
  disabilityType?: string;
  isPrimary: boolean;
}
