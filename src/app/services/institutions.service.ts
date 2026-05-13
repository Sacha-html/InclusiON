import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import {
  ApiResponse,
  CreateInstitutionRequest,
  InstitutionResponse,
  PagedResponse,
  UpdateInstitutionRequest,
} from '../models';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { unwrapResponse } from '@shared/utils';

export interface GetInstitutionsRequest {
  page?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class InstitutionsService {
  private readonly http = inject(HttpClient);

  private get apiUrl(): string {
    return `${environment.apiUrl}/Institutions`;
  }

  getPaged(request?: GetInstitutionsRequest): Observable<PagedResponse<InstitutionResponse>> {
    let params = new HttpParams();
    if (request?.page)     params = params.set('page', request.page.toString());
    if (request?.pageSize) params = params.set('pageSize', request.pageSize.toString());
    if (request?.search)   params = params.set('search', request.search);
    if (request?.isActive !== undefined) params = params.set('isActive', request.isActive.toString());

    return this.http
      .get<ApiResponse<PagedResponse<InstitutionResponse>>>(this.apiUrl, { params })
      .pipe(unwrapResponse());
  }

  getById(id: string): Observable<InstitutionResponse | undefined> {
    return this.getAll().pipe(map((list) => list?.find((i) => i.id.toString() === id)));
  }

  getAll(): Observable<InstitutionResponse[]> {
    return this.http
      .get<ApiResponse<PagedResponse<InstitutionResponse>>>(`${this.apiUrl}?pageSize=1000`)
      .pipe(unwrapResponse(), map((r) => r.data));
  }

  create(request: CreateInstitutionRequest): Observable<InstitutionResponse> {
    return this.http
      .post<ApiResponse<InstitutionResponse>>(this.apiUrl, request)
      .pipe(unwrapResponse());
  }

  update(id: string, request: UpdateInstitutionRequest): Observable<InstitutionResponse> {
    return this.http
      .put<ApiResponse<InstitutionResponse>>(`${this.apiUrl}/${id}`, request)
      .pipe(unwrapResponse());
  }

  patchStatus(id: string, isActive: boolean): Observable<InstitutionResponse> {
    return this.http
      .patch<ApiResponse<InstitutionResponse>>(`${this.apiUrl}/${id}`, { isActive })
      .pipe(unwrapResponse());
  }
}
