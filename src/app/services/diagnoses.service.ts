import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiResponse, PagedResponse } from '../models';
import { DiagnosisResponse, DiagnosisListItemResponse } from '../models/responses/diagnosis.response';
import { CreateDiagnosisRequest } from '../models/requests/diagnoses/create-diagnosis.request';
import { unwrapResponse, handleApiError } from '@shared/utils';

@Injectable({
  providedIn: 'root',
})
export class DiagnosesService {
  private readonly http = inject(HttpClient);

  private get baseUrl(): string {
    return environment.apiUrl;
  }

  getByPerson(personId: string, page = 1, pageSize = 100): Observable<DiagnosisListItemResponse[]> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http
      .get<ApiResponse<PagedResponse<DiagnosisListItemResponse>>>(`${this.baseUrl}/persons/${personId}/diagnoses`, { params })
      .pipe(unwrapResponse(), map((r) => r.data));
  }

  getById(id: string): Observable<DiagnosisResponse> {
    return this.http
      .get<ApiResponse<DiagnosisResponse>>(`${this.baseUrl}/diagnoses/${id}`)
      .pipe(unwrapResponse());
  }

  create(personId: string, request: CreateDiagnosisRequest): Observable<DiagnosisResponse> {
    return this.http
      .post<ApiResponse<DiagnosisResponse>>(`${this.baseUrl}/persons/${personId}/diagnoses`, request)
      .pipe(unwrapResponse());
  }

  update(id: string, request: CreateDiagnosisRequest): Observable<DiagnosisResponse> {
    return this.http
      .put<ApiResponse<DiagnosisResponse>>(`${this.baseUrl}/diagnoses/${id}`, request)
      .pipe(unwrapResponse());
  }

  patchStatus(id: string, isActive: boolean): Observable<void> {
    return this.http
      .patch<void>(`${this.baseUrl}/diagnoses/${id}`, { isActive })
      .pipe(handleApiError());
  }
}
