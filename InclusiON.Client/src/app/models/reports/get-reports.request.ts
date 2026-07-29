export interface GetReportsRequest {
  page: number;
  pageSize: number;
  search?: string;
  personId?: string;
  personIds?: string[];
  professionalId?: string;
  institutionId?: number;
  reportTypeId?: number;
  isActive?: boolean;
  onlyDeactivatedProfessionals?: boolean;
  status?: string;
  dateFrom?: string;
  dateTo?: string;
  sortBy?: string;
  sortDirection?: string;
}