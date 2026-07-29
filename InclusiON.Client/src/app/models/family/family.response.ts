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
  wasPreviouslyLinked?: boolean;
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
  relationship?: string;
}

export interface PersonRepresentativeResponse {
  personId: string;
  representativeId: string;
  representativeFullName: string;
  representativeDocumentNumber?: string;
  representativeEmail?: string;
  relationship?: string;
  isPrimary: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  endedAt?: string;
  unlinkObservation?: string;
}

export interface FamilyStatusHistoryResponse {
  id: string;
  familyId: string;
  oldStatus?: string;
  newStatus: string;
  observation?: string;
  changedByUserId?: string;
  changedByUserName?: string;
  createdAt: string;
}

export interface PersonRepresentativeHistoryResponse {
  id: string;
  personId: string;
  representativeId: string;
  representativeFullName: string;
  changeType: string;
  relationship?: string;
  wasPrimary?: boolean;
  observation?: string;
  createdAt: string;
}
