export interface GetFamilyRequest {
  page?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean;
  sortBy?: string;
  sortDirection?: string;
  institutionId?: number;
  linkedPersonSearch?: string;
}
