import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import {
  ApiResponse,
  CreateProfessionalRequest,
  GetProfessionalsRequest,
  PagedResponse,
  ProfessionalListItemResponse,
  ProfessionalResponse,
  UpdateProfessionalRequest,
} from '../models';
import { catchError, map, Observable, throwError } from 'rxjs';

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
    }

    return this.http
      .get<ApiResponse<PagedResponse<ProfessionalListItemResponse>>>(this.apiUrl, { params })
      .pipe(
        map((response) => response.data),
        catchError(this.handleError),
      );
  }

  getProfessionalById(id: string): Observable<ProfessionalResponse> {
    return this.http
      .get<ApiResponse<ProfessionalResponse>>(`${this.apiUrl}/${id}`)
      .pipe(
        map((response) => response.data),
        catchError(this.handleError),
      );
  }

  createProfessional(request: CreateProfessionalRequest): Observable<ProfessionalResponse> {
    return this.http
      .post<ApiResponse<ProfessionalResponse>>(this.apiUrl, request)
      .pipe(
        map((response) => response.data),
        catchError(this.handleError),
      );
  }

  updateProfessional(id: string, request: UpdateProfessionalRequest): Observable<ProfessionalResponse> {
    return this.http
      .put<ApiResponse<ProfessionalResponse>>(`${this.apiUrl}/${id}`, request)
      .pipe(
        map((response) => response.data),
        catchError(this.handleError),
      );
  }

  getMyProfile(): Observable<ProfessionalResponse> {
    return this.http
      .get<ApiResponse<ProfessionalResponse>>(`${this.apiUrl}/me`)
      .pipe(
        map((response) => response.data),
        catchError(this.handleError),
      );
  }

  deactivateProfessional(id: string): Observable<void> {
    return this.http
      .put<void>(`${this.apiUrl}/${id}/deactivate`, {})
      .pipe(catchError(this.handleError));
  }

  private handleError(error: unknown): Observable<never> {
    return throwError(() => error);
  }
}
