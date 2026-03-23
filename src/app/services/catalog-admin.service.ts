import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiResponse } from '@models';
import { environment } from '@env';

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
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/disability-types`, request).pipe(
      map(r => r.data),
      catchError(this.handleError),
    );
  }

  updateDisabilityType(id: number, request: { name: string; description?: string; isActive: boolean }): Observable<any> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/disability-types/${id}`, request).pipe(
      map(r => r.data),
      catchError(this.handleError),
    );
  }

  // Autonomy Levels
  createAutonomyLevel(request: { name: string; description?: string; requiresSupervision: boolean; displayOrder: number }): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/autonomy-levels`, request).pipe(
      map(r => r.data),
      catchError(this.handleError),
    );
  }

  updateAutonomyLevel(id: number, request: any): Observable<any> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/autonomy-levels/${id}`, request).pipe(
      map(r => r.data),
      catchError(this.handleError),
    );
  }

  // Activity Categories
  createActivityCategory(request: { name: string; description?: string }): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/activity-categories`, request).pipe(
      map(r => r.data),
      catchError(this.handleError),
    );
  }

  updateActivityCategory(id: number, request: any): Observable<any> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/activity-categories/${id}`, request).pipe(
      map(r => r.data),
      catchError(this.handleError),
    );
  }

  // Skill Areas
  createSkillArea(request: { name: string; description?: string; icon?: string; color?: string; displayOrder: number }): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/skill-areas`, request).pipe(
      map(r => r.data),
      catchError(this.handleError),
    );
  }

  updateSkillArea(id: number, request: any): Observable<any> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/skill-areas/${id}`, request).pipe(
      map(r => r.data),
      catchError(this.handleError),
    );
  }

  // Activity Template Types
  createActivityTemplateType(request: any): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/activity-template-types`, request).pipe(
      map(r => r.data),
      catchError(this.handleError),
    );
  }

  updateActivityTemplateType(id: number, request: any): Observable<any> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/activity-template-types/${id}`, request).pipe(
      map(r => r.data),
      catchError(this.handleError),
    );
  }

  // Login Methods
  updateLoginMethod(id: number, request: any): Observable<any> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/login-methods/${id}`, request).pipe(
      map(r => r.data),
      catchError(this.handleError),
    );
  }

  private handleError(error: unknown): Observable<never> {
    return throwError(() => error);
  }
}
