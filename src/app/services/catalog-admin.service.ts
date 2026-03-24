import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse } from '@models';
import { environment } from '@env';
import { unwrapResponse } from '@shared/utils';

@Injectable({
  providedIn: 'root',
})
export class CatalogAdminService {
  private readonly http = inject(HttpClient);

  private get apiUrl(): string {
    return `${environment.apiUrl}/admin/catalogs`;
  }

  // Disability Types
  createDisabilityType(request: { name: string; description?: string }): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/disability-types`, request).pipe(unwrapResponse());
  }

  updateDisabilityType(id: number, request: { name: string; description?: string; isActive: boolean }): Observable<any> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/disability-types/${id}`, request).pipe(unwrapResponse());
  }

  // Autonomy Levels
  createAutonomyLevel(request: { name: string; description?: string; requiresSupervision: boolean; displayOrder: number }): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/autonomy-levels`, request).pipe(unwrapResponse());
  }

  updateAutonomyLevel(id: number, request: any): Observable<any> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/autonomy-levels/${id}`, request).pipe(unwrapResponse());
  }

  // Activity Categories
  createActivityCategory(request: { name: string; description?: string }): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/activity-categories`, request).pipe(unwrapResponse());
  }

  updateActivityCategory(id: number, request: any): Observable<any> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/activity-categories/${id}`, request).pipe(unwrapResponse());
  }

  // Skill Areas
  createSkillArea(request: { name: string; description?: string; icon?: string; color?: string; displayOrder: number }): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/skill-areas`, request).pipe(unwrapResponse());
  }

  updateSkillArea(id: number, request: any): Observable<any> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/skill-areas/${id}`, request).pipe(unwrapResponse());
  }

  // Activity Template Types
  createActivityTemplateType(request: any): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/activity-template-types`, request).pipe(unwrapResponse());
  }

  updateActivityTemplateType(id: number, request: any): Observable<any> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/activity-template-types/${id}`, request).pipe(unwrapResponse());
  }

  // Login Methods
  updateLoginMethod(id: number, request: any): Observable<any> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/login-methods/${id}`, request).pipe(unwrapResponse());
  }
}
