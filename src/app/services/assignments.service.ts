import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import {
  ApiResponse,
  AssignInstitutionRequest,
  AssignPersonRequest,
  ProfessionalInstitutionResponse,
  ProfessionalPersonResponse,
} from '../models';
import { catchError, map, Observable, throwError } from 'rxjs';

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
      .pipe(
        map((response) => response.data),
        catchError(this.handleError),
      );
  }

  assignPerson(profId: string, request: AssignPersonRequest): Observable<ProfessionalPersonResponse> {
    return this.http
      .post<ApiResponse<ProfessionalPersonResponse>>(`${this.professionalsUrl(profId)}/persons`, request)
      .pipe(
        map((response) => response.data),
        catchError(this.handleError),
      );
  }

  deactivatePersonAssignment(profId: string, personId: string): Observable<void> {
    return this.http
      .put<void>(`${this.professionalsUrl(profId)}/persons/${personId}/deactivate`, {})
      .pipe(catchError(this.handleError));
  }

  getInstitutionsByProfessional(profId: string): Observable<ProfessionalInstitutionResponse[]> {
    return this.http
      .get<ApiResponse<ProfessionalInstitutionResponse[]>>(`${this.professionalsUrl(profId)}/institutions`)
      .pipe(
        map((response) => response.data),
        catchError(this.handleError),
      );
  }

  assignInstitution(profId: string, request: AssignInstitutionRequest): Observable<ProfessionalInstitutionResponse> {
    return this.http
      .post<ApiResponse<ProfessionalInstitutionResponse>>(`${this.professionalsUrl(profId)}/institutions`, request)
      .pipe(
        map((response) => response.data),
        catchError(this.handleError),
      );
  }

  removeInstitutionAssignment(profId: string, instId: number): Observable<void> {
    return this.http
      .delete<void>(`${this.professionalsUrl(profId)}/institutions/${instId}`)
      .pipe(catchError(this.handleError));
  }

  private handleError(error: unknown): Observable<never> {
    return throwError(() => error);
  }
}
