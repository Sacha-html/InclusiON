export interface ProfessionalListItemResponse {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  documentNumber?: string;
  phone?: string;
  specialty?: string;
  licenseNumber?: string;
  isActive: boolean;
  status: string;
  email?: string;
}

export interface ProfessionalResponse {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  documentNumber?: string;
  phone?: string;
  specialty?: string;
  licenseNumber?: string;
  birthDate?: string;
  isActive: boolean;
  /** Numeric status code returned by detail endpoint (0=Pending, 1=Approved, 2=Rejected, 3=Suspended, 4=Terminated) */
  status?: number;
  /** Spanish display name returned by detail endpoint ("Pendiente", "Aprobado", "Rechazado", "Suspendido", "Dado de baja") */
  statusName?: string;
  createdAt: string;
  updatedAt?: string;
  temporaryPassword?: string;
  email?: string;
}
