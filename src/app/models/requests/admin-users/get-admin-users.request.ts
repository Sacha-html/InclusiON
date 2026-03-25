export interface GetAdminUsersRequest {
  page?: number;
  pageSize?: number;
  search?: string;
  role?: string;
  isActive?: boolean;
  institutionId?: number;
  sortBy?: string;
  sortDirection?: string;
}
