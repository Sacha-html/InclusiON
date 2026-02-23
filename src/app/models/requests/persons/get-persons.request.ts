export interface GetPersonsRequest {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'ASC' | 'DESC';

  // Filtros
  search?: string;
  disabilityTypeId?: number;
  autonomyLevelId?: number;
  isActive?: boolean;
}
