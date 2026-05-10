import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import {
  ApiResponse,
  CreateFamilyRequest,
  UpdateFamilyRequest,
  GetFamilyRequest,
  PagedResponse,
  FamilyListItemResponse,
  FamilyDashboardResponse,
  FamilyResponse,
  PersonRepresentativeResponse,
  FamilyStatusHistoryResponse,
  PersonRepresentativeHistoryResponse,
} from '../models';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { unwrapResponse, handleApiError } from '@shared/utils';

@Injectable({
  providedIn: 'root',
})
export class FamilyService {
  private readonly http = inject(HttpClient);

  private get apiUrl(): string {
    return `${environment.apiUrl}/Family`;
  }

  getFamily(request?: GetFamilyRequest): Observable<PagedResponse<FamilyListItemResponse>> {
    let params = new HttpParams()
      .set('sortBy', request?.sortBy ?? 'lastName')
      .set('sortDirection', request?.sortDirection ?? 'ASC');

    if (request) {
      if (request.page) params = params.set('page', request.page.toString());
      if (request.pageSize) params = params.set('pageSize', request.pageSize.toString());
      if (request.search) params = params.set('search', request.search);
      if (request.institutionId) params = params.set('institutionId', request.institutionId.toString());
      if (request.linkedPersonSearch) params = params.set('linkedPersonSearch', request.linkedPersonSearch);
      if (request.isActive !== undefined) params = params.set('isActive', request.isActive.toString());
    }

    return this.http
      .get<ApiResponse<PagedResponse<FamilyListItemResponse>>>(this.apiUrl, { params })
      .pipe(unwrapResponse());
  }

  getFamilyById(id: string): Observable<FamilyResponse> {
    return this.http
      .get<ApiResponse<FamilyResponse>>(`${this.apiUrl}/${id}`)
      .pipe(unwrapResponse());
  }

  createFamily(request: CreateFamilyRequest): Observable<FamilyResponse> {
    return this.http
      .post<ApiResponse<FamilyResponse>>(this.apiUrl, request)
      .pipe(unwrapResponse());
  }

  updateFamily(id: string, request: UpdateFamilyRequest): Observable<FamilyResponse> {
    return this.http
      .put<ApiResponse<FamilyResponse>>(`${this.apiUrl}/${id}`, request)
      .pipe(unwrapResponse());
  }

  deactivateFamily(id: string): Observable<void> {
    return this.http
      .put<void>(`${this.apiUrl}/${id}/deactivate`, {})
      .pipe(handleApiError());
  }

  getAvailableFamilies(search?: string, page = 1, pageSize = 50): Observable<FamilyResponse[]> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    if (search) params = params.set('search', search);

    return this.http
      .get<ApiResponse<PagedResponse<FamilyResponse>>>(`${this.apiUrl}/available`, { params })
      .pipe(unwrapResponse(), map((r) => r.data));
  }

  getPersonRepresentatives(personId: string): Observable<PersonRepresentativeResponse[]> {
    return this.http
      .get<ApiResponse<PersonRepresentativeResponse[]>>(`${environment.apiUrl}/Persons/${personId}/representatives`)
      .pipe(unwrapResponse());
  }

  linkFamilyToPerson(familyId: string, personId: string, request: { relationship: string; isPrimary: boolean }): Observable<PersonRepresentativeResponse> {
    return this.http
      .post<ApiResponse<PersonRepresentativeResponse>>(`${this.apiUrl}/${familyId}/link/${personId}`, request)
      .pipe(unwrapResponse());
  }

  unlinkFamilyFromPerson(familyId: string, personId: string, observation: string): Observable<PersonRepresentativeResponse> {
    return this.http
      .delete<ApiResponse<PersonRepresentativeResponse>>(`${this.apiUrl}/${familyId}/unlink/${personId}`, { body: { observation } })
      .pipe(unwrapResponse());
  }

  getFamilyStatusHistory(familyId: string): Observable<FamilyStatusHistoryResponse[]> {
    return this.http
      .get<ApiResponse<FamilyStatusHistoryResponse[]>>(`${this.apiUrl}/${familyId}/status-history`)
      .pipe(unwrapResponse());
  }

  getFamilyLinkHistory(familyId: string): Observable<PersonRepresentativeHistoryResponse[]> {
    return this.http
      .get<ApiResponse<PersonRepresentativeHistoryResponse[]>>(`${this.apiUrl}/${familyId}/link-history`)
      .pipe(unwrapResponse());
  }

  getPersonLinkHistory(personId: string): Observable<PersonRepresentativeHistoryResponse[]> {
    return this.http
      .get<ApiResponse<PersonRepresentativeHistoryResponse[]>>(`${environment.apiUrl}/Persons/${personId}/link-history`)
      .pipe(unwrapResponse());
  }

  // Professional endpoints
  getAvailableFamiliesForProfessional(search?: string, personId?: string, page = 1, pageSize = 50): Observable<FamilyResponse[]> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    if (search) params = params.set('search', search);
    if (personId) params = params.set('personId', personId);

    return this.http
      .get<ApiResponse<PagedResponse<FamilyResponse>>>(`${this.apiUrl}/professional/available`, { params })
      .pipe(unwrapResponse(), map((r) => r.data));
  }

  linkFamilyToPersonAsProfessional(familyId: string, personId: string, request: { relationship: string; isPrimary: boolean }): Observable<PersonRepresentativeResponse> {
    return this.http
      .post<ApiResponse<PersonRepresentativeResponse>>(`${this.apiUrl}/professional/link/${familyId}/${personId}`, request)
      .pipe(unwrapResponse());
  }

  unlinkFamilyFromPersonAsProfessional(familyId: string, personId: string, observation: string): Observable<PersonRepresentativeResponse> {
    return this.http
      .delete<ApiResponse<PersonRepresentativeResponse>>(`${this.apiUrl}/professional/unlink/${familyId}/${personId}`, { body: { observation } })
      .pipe(unwrapResponse());
  }

  getDashboard(): Observable<FamilyDashboardResponse> {
    return this.http
      .get<ApiResponse<FamilyDashboardResponse>>(`${this.apiUrl}/dashboard`)
      .pipe(unwrapResponse());
  }
}
