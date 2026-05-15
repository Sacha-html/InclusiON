import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import {
  ApiResponse,
  CreateProfessionalRequest,
  DeactivateProfessionalRequest,
  GetProfessionalsRequest,
  PagedResponse,
  ProfessionalListItemResponse,
  ProfessionalResponse,
  RegisterProfessionalRequest,
  UpdateProfessionalRequest,
  ValidateProfessionalRequest,
} from '@models';
import { Observable } from 'rxjs';
import { unwrapResponse, handleApiError } from '@shared/utils';

@Injectable({
  providedIn: 'root',
})
export class ProfessionalsService {
  private readonly http = inject(HttpClient);

  private get apiUrl(): string {
    return `${environment.apiUrl}/Professionals`;
  }

  getProfessionals(
    request?: GetProfessionalsRequest,
  ): Observable<PagedResponse<ProfessionalListItemResponse>> {
    let params = new HttpParams()
      .set('sortBy', request?.sortBy ?? 'lastName')
      .set('sortDirection', request?.sortDirection ?? 'ASC');

    if (request) {
      if (request.page) params = params.set('page', request.page.toString());
      if (request.pageSize) params = params.set('pageSize', request.pageSize.toString());
      if (request.search) params = params.set('search', request.search);
      if (request.institutionId) params = params.set('institutionId', request.institutionId.toString());
      if (request.status) params = params.set('status', request.status);
      if (request.isActive !== undefined) params = params.set('isActive', request.isActive.toString());
    }

    return this.http
      .get<ApiResponse<PagedResponse<ProfessionalListItemResponse>>>(this.apiUrl, { params })
      .pipe(unwrapResponse());
  }

  getProfessionalById(id: string): Observable<ProfessionalResponse> {
    return this.http
      .get<ApiResponse<ProfessionalResponse>>(`${this.apiUrl}/${id}`)
      .pipe(unwrapResponse());
  }

  createProfessional(request: CreateProfessionalRequest): Observable<ProfessionalResponse> {
    return this.http
      .post<ApiResponse<ProfessionalResponse>>(this.apiUrl, request)
      .pipe(unwrapResponse());
  }

  updateProfessional(id: string, request: UpdateProfessionalRequest): Observable<ProfessionalResponse> {
    return this.http
      .put<ApiResponse<ProfessionalResponse>>(`${this.apiUrl}/${id}`, request)
      .pipe(unwrapResponse());
  }

  getMyProfile(): Observable<ProfessionalResponse> {
    return this.http
      .get<ApiResponse<ProfessionalResponse>>(`${this.apiUrl}/me`)
      .pipe(unwrapResponse());
  }

  deactivateProfessional(id: string, request?: DeactivateProfessionalRequest): Observable<void> {
    return this.http
      .put<void>(`${this.apiUrl}/${id}/deactivate`, request ?? {})
      .pipe(handleApiError());
  }

  registerProfessional(request: RegisterProfessionalRequest): Observable<void> {
    return this.http
      .post<void>(`${environment.apiUrl}/Professionals/register`, request)
      .pipe(handleApiError());
  }

  getPendingProfessionals(
    request?: GetProfessionalsRequest,
  ): Observable<PagedResponse<ProfessionalListItemResponse>> {
    let params = new HttpParams()
      .set('sortBy', request?.sortBy ?? 'createdAt')
      .set('sortDirection', request?.sortDirection ?? 'DESC');

    if (request) {
      if (request.page) params = params.set('page', request.page.toString());
      if (request.pageSize) params = params.set('pageSize', request.pageSize.toString());
      if (request.search) params = params.set('search', request.search);
    }

    return this.http
      .get<ApiResponse<PagedResponse<ProfessionalListItemResponse>>>(`${this.apiUrl}/pending`, { params })
      .pipe(unwrapResponse());
  }

  validateProfessional(id: string, request: ValidateProfessionalRequest): Observable<void> {
    return this.http
      .put<void>(`${this.apiUrl}/${id}/validate`, request)
      .pipe(handleApiError());
  }

  reactivateProfessional(id: string): Observable<void> {
    return this.http
      .put<void>(`${this.apiUrl}/${id}/reactivate`, {})
      .pipe(handleApiError());
  }

  getStatusHistory(id: string): Observable<any[]> {
    return this.http
      .get<ApiResponse<any[]>>(`${this.apiUrl}/${id}/status-history`)
      .pipe(unwrapResponse());
  }

  checkEmail(email: string): Observable<{ isAvailable: boolean; message?: string }> {
    return this.http
      .get<{ isAvailable: boolean; message?: string }>(`${environment.apiUrl}/ProfessionalValidation/email?email=${encodeURIComponent(email)}`);
  }

  checkLicenseNumber(licenseNumber: string): Observable<{ isAvailable: boolean; message?: string }> {
    return this.http
      .get<{ isAvailable: boolean; message?: string }>(`${environment.apiUrl}/ProfessionalValidation/license-number?licenseNumber=${encodeURIComponent(licenseNumber)}`);
  }
}
