export interface CreateFamilyRequest {
  firstName: string;
  lastName: string;
  email: string;
  documentNumber?: string;
  phone?: string;
  relationship?: string;
}
