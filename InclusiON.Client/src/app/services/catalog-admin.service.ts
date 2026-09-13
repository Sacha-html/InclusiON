import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse, CatalogItem, AutonomyLevelItem, ActivityCategoryItem, SkillAreaItem, ActivityTemplateTypeItem, LoginMethodItem, SpecialtyItem } from '@models';
import { environment } from '@env';
import { unwrapResponse } from '@shared/utils';

export interface ActivityTemplateTypeFormValue {
  skillAreaId: number;
  name: string;
  code: string;
  usesPictograms: boolean;
  hasAudio: boolean;
  isActive?: boolean;
}

export interface ActivityTemplateTypeTechnicalValues {
  contentSchema?: string;
  componentName?: string;
  displayOrder?: number;
}

export interface CreateActivityTemplateTypeRequest extends ActivityTemplateTypeFormValue {
  contentSchema: string;
  componentName: string;
  displayOrder: number;
}

export interface UpdateActivityTemplateTypeRequest extends CreateActivityTemplateTypeRequest {
  isActive: boolean;
}

// These values are backend metadata, not administrator-editable Angular components.
export function buildActivityTemplateTypePayload(
  value: ActivityTemplateTypeFormValue,
  technicalValues?: ActivityTemplateTypeTechnicalValues,
): CreateActivityTemplateTypeRequest | UpdateActivityTemplateTypeRequest {
  const code = value.code.trim();
  const componentName = technicalValues?.componentName?.trim() || code || 'TemplateType';
  const { isActive, ...formFields } = value;
  const payload = {
    ...formFields,
    code,
    contentSchema: technicalValues?.contentSchema ?? '{}',
    componentName,
    displayOrder: technicalValues?.displayOrder ?? 0,
  };

  return isActive === undefined ? payload : { ...payload, isActive };
}

@Injectable({
  providedIn: 'root',
})
export class CatalogAdminService {
  private readonly http = inject(HttpClient);

  private get apiUrl(): string {
    return `${environment.apiUrl}/admin/catalogs`;
  }

  // Disability Types
  getAllSpecialties(): Observable<SpecialtyItem[]> { return this.http.get<ApiResponse<SpecialtyItem[]>>(`${this.apiUrl}/specialties`).pipe(unwrapResponse()); }
  createSpecialty(request: { name: string }): Observable<SpecialtyItem> { return this.http.post<ApiResponse<SpecialtyItem>>(`${this.apiUrl}/specialties`, request).pipe(unwrapResponse()); }
  updateSpecialty(id: string, request: { name: string; isActive: boolean }): Observable<SpecialtyItem> { return this.http.put<ApiResponse<SpecialtyItem>>(`${this.apiUrl}/specialties/${id}`, request).pipe(unwrapResponse()); }
  patchSpecialtyStatus(id: string, isActive: boolean): Observable<SpecialtyItem> { return this.http.patch<ApiResponse<SpecialtyItem>>(`${this.apiUrl}/specialties/${id}`, { isActive }).pipe(unwrapResponse()); }
  createDisabilityType(request: { name: string; description?: string }): Observable<CatalogItem> {
    return this.http.post<ApiResponse<CatalogItem>>(`${this.apiUrl}/disability-types`, request).pipe(unwrapResponse());
  }

  updateDisabilityType(id: string, request: { name: string; description?: string; isActive: boolean }): Observable<CatalogItem> {
    return this.http.put<ApiResponse<CatalogItem>>(`${this.apiUrl}/disability-types/${id}`, request).pipe(unwrapResponse());
  }

  // Autonomy Levels
  createAutonomyLevel(request: { name: string; description?: string; requiresSupervision: boolean; displayOrder: number }): Observable<AutonomyLevelItem> {
    return this.http.post<ApiResponse<AutonomyLevelItem>>(`${this.apiUrl}/autonomy-levels`, request).pipe(unwrapResponse());
  }

  updateAutonomyLevel(id: string, request: Record<string, unknown>): Observable<AutonomyLevelItem> {
    return this.http.put<ApiResponse<AutonomyLevelItem>>(`${this.apiUrl}/autonomy-levels/${id}`, request).pipe(unwrapResponse());
  }

  // Activity Categories
  createActivityCategory(request: { name: string; description?: string }): Observable<ActivityCategoryItem> {
    return this.http.post<ApiResponse<ActivityCategoryItem>>(`${this.apiUrl}/activity-categories`, request).pipe(unwrapResponse());
  }

  updateActivityCategory(id: string, request: Record<string, unknown>): Observable<ActivityCategoryItem> {
    return this.http.put<ApiResponse<ActivityCategoryItem>>(`${this.apiUrl}/activity-categories/${id}`, request).pipe(unwrapResponse());
  }

  // Skill Areas
  createSkillArea(request: { name: string; description?: string; icon?: string; color?: string; displayOrder: number }): Observable<SkillAreaItem> {
    return this.http.post<ApiResponse<SkillAreaItem>>(`${this.apiUrl}/skill-areas`, request).pipe(unwrapResponse());
  }

  updateSkillArea(id: string, request: Record<string, unknown>): Observable<SkillAreaItem> {
    return this.http.put<ApiResponse<SkillAreaItem>>(`${this.apiUrl}/skill-areas/${id}`, request).pipe(unwrapResponse());
  }

  // Activity Template Types
  createActivityTemplateType(request: ActivityTemplateTypeFormValue): Observable<ActivityTemplateTypeItem> {
    const payload = buildActivityTemplateTypePayload(request);
    return this.http.post<ApiResponse<ActivityTemplateTypeItem>>(`${this.apiUrl}/activity-template-types`, payload).pipe(unwrapResponse());
  }

  updateActivityTemplateType(id: string, request: ActivityTemplateTypeFormValue, technicalValues?: ActivityTemplateTypeTechnicalValues): Observable<ActivityTemplateTypeItem> {
    const payload = buildActivityTemplateTypePayload(request, technicalValues) as UpdateActivityTemplateTypeRequest;
    return this.http.put<ApiResponse<ActivityTemplateTypeItem>>(`${this.apiUrl}/activity-template-types/${id}`, payload).pipe(unwrapResponse());
  }

  // Login Methods
  updateLoginMethod(id: string, request: Record<string, unknown>): Observable<LoginMethodItem> {
    return this.http.put<ApiResponse<LoginMethodItem>>(`${this.apiUrl}/login-methods/${id}`, request).pipe(unwrapResponse());
  }

  // Patch status (state machine)
  getAllReportTypes(): Observable<CatalogItem[]> {
    return this.http.get<ApiResponse<CatalogItem[]>>(`${this.apiUrl}/report-types`).pipe(unwrapResponse());
  }

  createReportType(request: { name: string; description?: string }): Observable<CatalogItem> {
    return this.http.post<ApiResponse<CatalogItem>>(`${this.apiUrl}/report-types`, request).pipe(unwrapResponse());
  }

  updateReportType(id: string, request: { name: string; description?: string; isActive: boolean }): Observable<CatalogItem> {
    return this.http.put<ApiResponse<CatalogItem>>(`${this.apiUrl}/report-types/${id}`, request).pipe(unwrapResponse());
  }

  patchReportTypeStatus(id: string, isActive: boolean): Observable<CatalogItem> {
    return this.http.patch<ApiResponse<CatalogItem>>(`${this.apiUrl}/report-types/${id}`, { isActive }).pipe(unwrapResponse());
  }

  patchDisabilityTypeStatus(id: string, isActive: boolean): Observable<CatalogItem> {
    return this.http.patch<ApiResponse<CatalogItem>>(`${this.apiUrl}/disability-types/${id}`, { isActive }).pipe(unwrapResponse());
  }

  patchAutonomyLevelStatus(id: string, isActive: boolean): Observable<AutonomyLevelItem> {
    return this.http.patch<ApiResponse<AutonomyLevelItem>>(`${this.apiUrl}/autonomy-levels/${id}`, { isActive }).pipe(unwrapResponse());
  }

  patchActivityCategoryStatus(id: string, isActive: boolean): Observable<ActivityCategoryItem> {
    return this.http.patch<ApiResponse<ActivityCategoryItem>>(`${this.apiUrl}/activity-categories/${id}`, { isActive }).pipe(unwrapResponse());
  }

  patchSkillAreaStatus(id: string, isActive: boolean): Observable<SkillAreaItem> {
    return this.http.patch<ApiResponse<SkillAreaItem>>(`${this.apiUrl}/skill-areas/${id}`, { isActive }).pipe(unwrapResponse());
  }

  patchActivityTemplateTypeStatus(id: string, isActive: boolean): Observable<ActivityTemplateTypeItem> {
    return this.http.patch<ApiResponse<ActivityTemplateTypeItem>>(`${this.apiUrl}/activity-template-types/${id}`, { isActive }).pipe(unwrapResponse());
  }
}
