export interface CreatePersonRequest {
  firstName: string;
  lastName: string;
  documentNumber?: string;
  birthDate: string; // ISO date string

  photoUrl?: string;

}
