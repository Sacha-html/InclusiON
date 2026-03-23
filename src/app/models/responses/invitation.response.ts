export interface InvitationResponse {
  id: number;
  code: string;
  email: string;
  firstName?: string;
  lastName?: string;
  relationship?: string;
  personName?: string;
  expiresAt: string;
  isUsed: boolean;
  usedAt?: string;
  status: 'Enviada' | 'Aceptada' | 'Expirada';
  createdByProfessionalName?: string;
  createdAt: string;
}

export interface InvitationValidationResponse {
  code: string;
  email: string;
  firstName?: string;
  lastName?: string;
  relationship?: string;
  personName?: string;
}

export interface AcceptInvitationResponse {
  success: boolean;
  message: string;
}
