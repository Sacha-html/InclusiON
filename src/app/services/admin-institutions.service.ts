import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import { AdminInstitutionResponse, ApiResponse } from '../models';
import { Observable } from 'rxjs';
import { unwrapResponse, handleApiError } from '@shared/utils';

@Injectable({
  providedIn: 'root',
})
export class AdminInstitutionsService {
  private readonly http = inject(HttpClient);

  private get apiUrl(): string {
    return `${environment.apiUrl}/admin/institutions-assignments`;
  }

  getMyInstitutions(): Observable<AdminInstitutionResponse[]> {
    return this.http
      .get<ApiResponse<AdminInstitutionResponse[]>>(`${this.apiUrl}/me`)
      .pipe(unwrapResponse());
  }

  getByAdmin(adminUserId: string): Observable<AdminInstitutionResponse[]> {
    return this.http
      .get<ApiResponse<AdminInstitutionResponse[]>>(`${this.apiUrl}/${adminUserId}`)
      .pipe(unwrapResponse());
  }

  assign(adminUserId: string, institutionId: number): Observable<AdminInstitutionResponse> {
    return this.http
      .post<ApiResponse<AdminInstitutionResponse>>(`${this.apiUrl}/${adminUserId}`, { institutionId })
      .pipe(unwrapResponse());
  }

  remove(adminUserId: string, institutionId: number): Observable<void> {
    return this.http
      .delete<void>(`${this.apiUrl}/${adminUserId}/${institutionId}`)
      .pipe(handleApiError());
  }
}
