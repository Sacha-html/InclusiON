export interface GetProfessionalsRequest {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'ASC' | 'DESC';

  // Filtros
  search?: string;
  specialty?: string;
  isActive?: boolean;
  status?: string;
  institutionId?: number;
}
