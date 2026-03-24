import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import {
  ApiResponse,
  CreateInstitutionRequest,
  InstitutionResponse,
  UpdateInstitutionRequest,
} from '../models';
import { Observable } from 'rxjs';
import { unwrapResponse } from '@shared/utils';

@Injectable({
  providedIn: 'root',
})
export class InstitutionsService {
  private readonly http = inject(HttpClient);

  private get apiUrl(): string {
    return `${environment.apiUrl}/Institutions`;
  }

  getAll(): Observable<InstitutionResponse[]> {
    return this.http
      .get<ApiResponse<InstitutionResponse[]>>(this.apiUrl)
      .pipe(unwrapResponse());
  }

  create(request: CreateInstitutionRequest): Observable<InstitutionResponse> {
    return this.http
      .post<ApiResponse<InstitutionResponse>>(this.apiUrl, request)
      .pipe(unwrapResponse());
  }

  update(id: number, request: UpdateInstitutionRequest): Observable<InstitutionResponse> {
    return this.http
      .put<ApiResponse<InstitutionResponse>>(`${this.apiUrl}/${id}`, request)
      .pipe(unwrapResponse());
  }
}
