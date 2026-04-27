export interface GetReportsRequest {
  page: number;
  pageSize: number;
  search?: string;
  personId?: string;
  professionalId?: string;
  reportTypeId?: number;
  isActive?: boolean;
  status?: string;
  dateFrom?: string;
  dateTo?: string;
  sortBy?: string;
  sortDirection?: string;
}