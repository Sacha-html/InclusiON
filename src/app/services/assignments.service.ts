import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import {
  ApiResponse,
  AssignInstitutionRequest,
  AssignPersonRequest,
  ProfessionalInstitutionResponse,
  ProfessionalPersonResponse,
} from '@models';
import { Observable } from 'rxjs';
import { unwrapResponse, handleApiError } from '@shared/utils';

@Injectable({
  providedIn: 'root',
})
export class AssignmentsService {
  private readonly http = inject(HttpClient);

  private professionalsUrl(profId: string): string {
    return `${environment.apiUrl}/Professionals/${profId}`;
  }

  getPersonsByProfessional(profId: string): Observable<ProfessionalPersonResponse[]> {
    return this.http
      .get<ApiResponse<ProfessionalPersonResponse[]>>(`${this.professionalsUrl(profId)}/persons`)
      .pipe(unwrapResponse());
  }

  assignPerson(profId: string, request: AssignPersonRequest): Observable<ProfessionalPersonResponse> {
    return this.http
      .post<ApiResponse<ProfessionalPersonResponse>>(`${this.professionalsUrl(profId)}/persons`, request)
      .pipe(unwrapResponse());
  }

  deactivatePersonAssignment(profId: string, personId: string): Observable<void> {
    return this.http
      .put<void>(`${this.professionalsUrl(profId)}/persons/${personId}/deactivate`, {})
      .pipe(handleApiError());
  }

  getInstitutionsByProfessional(profId: string): Observable<ProfessionalInstitutionResponse[]> {
    return this.http
      .get<ApiResponse<ProfessionalInstitutionResponse[]>>(`${this.professionalsUrl(profId)}/institutions`)
      .pipe(unwrapResponse());
  }

  assignInstitution(profId: string, request: AssignInstitutionRequest): Observable<ProfessionalInstitutionResponse> {
    return this.http
      .post<ApiResponse<ProfessionalInstitutionResponse>>(`${this.professionalsUrl(profId)}/institutions`, request)
      .pipe(unwrapResponse());
  }

  removeInstitutionAssignment(profId: string, instId: string): Observable<void> {
    return this.http
      .delete<void>(`${this.professionalsUrl(profId)}/institutions/${instId}`)
      .pipe(handleApiError());
  }
}
