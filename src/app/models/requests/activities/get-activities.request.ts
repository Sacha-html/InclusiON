export interface GetActivitiesRequest {
  page?: number;
  pageSize?: number;
  search?: string;
  categoryId?: number;
  skillAreaId?: number;
  templateTypeId?: number;
  isActive?: boolean;
  isStandard?: boolean;
  sortBy?: string;
  sortDirection?: string;
}
