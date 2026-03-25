import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models';
import { DiagnosisResponse, DiagnosisListItemResponse } from '../models/responses/diagnosis.response';
import { CreateDiagnosisRequest } from '../models/requests/diagnoses/create-diagnosis.request';
import { unwrapResponse } from '@shared/utils';

@Injectable({
  providedIn: 'root',
})
export class DiagnosesService {
  private readonly http = inject(HttpClient);

  private get baseUrl(): string {
    return environment.apiUrl;
  }

  getByPerson(personId: string): Observable<DiagnosisListItemResponse[]> {
    return this.http
      .get<ApiResponse<DiagnosisListItemResponse[]>>(`${this.baseUrl}/persons/${personId}/diagnoses`)
      .pipe(unwrapResponse());
  }

  getById(id: number): Observable<DiagnosisResponse> {
    return this.http
      .get<ApiResponse<DiagnosisResponse>>(`${this.baseUrl}/diagnoses/${id}`)
      .pipe(unwrapResponse());
  }

  create(personId: string, request: CreateDiagnosisRequest): Observable<DiagnosisResponse> {
    return this.http
      .post<ApiResponse<DiagnosisResponse>>(`${this.baseUrl}/persons/${personId}/diagnoses`, request)
      .pipe(unwrapResponse());
  }

  update(id: number, request: CreateDiagnosisRequest): Observable<DiagnosisResponse> {
    return this.http
      .put<ApiResponse<DiagnosisResponse>>(`${this.baseUrl}/diagnoses/${id}`, request)
      .pipe(unwrapResponse());
  }
}
