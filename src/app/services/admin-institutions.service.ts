import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import { AdminInstitutionResponse, AdminUserResponse, ApiResponse, PagedResponse } from '../models';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { unwrapResponse, handleApiError } from '@shared/utils';

@Injectable({
  providedIn: 'root',
})
export class AdminInstitutionsService {
  private readonly http = inject(HttpClient);

  private get apiUrl(): string {
    return `${environment.apiUrl}/admin/institutions-assignments`;
  }

  getAdmins(page = 1, pageSize = 10, search?: string): Observable<PagedResponse<AdminUserResponse>> {
    let url = `${this.apiUrl}/admins?page=${page}&pageSize=${pageSize}`;
    if (search) url += `&search=${encodeURIComponent(search)}`;
    return this.http
      .get<ApiResponse<PagedResponse<AdminUserResponse>>>(url)
      .pipe(unwrapResponse());
  }

  getMyInstitutions(): Observable<AdminInstitutionResponse[]> {
    return this.http
      .get<ApiResponse<PagedResponse<AdminInstitutionResponse>>>(`${this.apiUrl}/me?pageSize=200`)
      .pipe(unwrapResponse(), map((r) => r.data));
  }

  getByAdmin(adminUserId: string): Observable<AdminInstitutionResponse[]> {
    return this.http
      .get<ApiResponse<PagedResponse<AdminInstitutionResponse>>>(`${this.apiUrl}/${adminUserId}?pageSize=200`)
      .pipe(unwrapResponse(), map((r) => r.data));
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
