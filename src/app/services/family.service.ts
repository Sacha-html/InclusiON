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
  FamilyResponse,
} from '../models';
import { Observable } from 'rxjs';
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
}
