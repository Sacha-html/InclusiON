export interface GetReportsRequest {
  page: number;
  pageSize: number;
  search?: string;
  personId?: string;
  professionalId?: string;
  reportTypeId?: string;
  isActive?: boolean;
  sortBy?: string;
  sortDirection?: string;
}