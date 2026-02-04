import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import {
  ApiResponse,
  PagedResponse,
  PersonResponse,
  PersonListItemResponse,
  CreatePersonRequest,
  UpdatePersonRequest,
  GetPersonsRequest,
} from '@models';
import { environment } from '@env';

@Injectable({
  providedIn: 'root',
})
export class PersonsService {
  private http = inject(HttpClient);

  private get apiUrl(): string {
    return `${environment.apiUrl}/Persons`;
  }

  /**
   * Obtiene la lista paginada de personas con filtros opcionales.
   * Requiere rol ProfessionalOrAbove.
   */
  getPersons(
    request?: GetPersonsRequest
  ): Observable<ApiResponse<PagedResponse<PersonListItemResponse>>> {
    let params = new HttpParams();

    if (request) {
      if (request.page) {
        params = params.set('page', request.page.toString());
      }
      if (request.pageSize) {
        params = params.set('pageSize', request.pageSize.toString());
      }
      if (request.sortBy) {
        params = params.set('sortBy', request.sortBy);
      }
      if (request.sortDirection) {
        params = params.set('sortDirection', request.sortDirection);
      }
      if (request.search) {
        params = params.set('search', request.search);
      }
      if (request.disabilityTypeId !== undefined) {
        params = params.set(
          'disabilityTypeId',
          request.disabilityTypeId.toString()
        );
      }
      if (request.autonomyLevelId !== undefined) {
        params = params.set(
          'autonomyLevelId',
          request.autonomyLevelId.toString()
        );
      }
      if (request.isActive !== undefined) {
        params = params.set('isActive', request.isActive.toString());
      }
    }

    return this.http
      .get<ApiResponse<PagedResponse<PersonListItemResponse>>>(this.apiUrl, {
        params,
      })
      .pipe(catchError(this.handleError));
  }

  /**
   * Obtiene una persona por su ID.
   * Requiere rol ValidUser.
   */
  getPersonById(personId: string): Observable<ApiResponse<PersonResponse>> {
    return this.http
      .get<ApiResponse<PersonResponse>>(`${this.apiUrl}/${personId}`)
      .pipe(catchError(this.handleError));
  }

  /**
   * Obtiene el perfil de la persona autenticada.
   * Requiere autenticación.
   */
  getMyProfile(): Observable<ApiResponse<PersonResponse>> {
    return this.http
      .get<ApiResponse<PersonResponse>>(`${this.apiUrl}/me`)
      .pipe(catchError(this.handleError));
  }

  /**
   * Crea una nueva persona con discapacidad.
   * Requiere rol ProfessionalOrAbove.
   */
  createPerson(
    request: CreatePersonRequest
  ): Observable<ApiResponse<PersonResponse>> {
    return this.http
      .post<ApiResponse<PersonResponse>>(this.apiUrl, request)
      .pipe(catchError(this.handleError));
  }

  /**
   * Actualiza una persona existente.
   * Requiere rol ProfessionalOrAbove.
   */
  updatePerson(
    personId: string,
    request: UpdatePersonRequest
  ): Observable<ApiResponse<PersonResponse>> {
    return this.http
      .put<ApiResponse<PersonResponse>>(`${this.apiUrl}/${personId}`, request)
      .pipe(catchError(this.handleError));
  }

  /**
   * Actualiza el perfil de la persona autenticada.
   * Requiere autenticación.
   */
  updateMyProfile(
    request: UpdatePersonRequest
  ): Observable<ApiResponse<PersonResponse>> {
    return this.http
      .put<ApiResponse<PersonResponse>>(`${this.apiUrl}/me`, request)
      .pipe(catchError(this.handleError));
  }

  private handleError(error: unknown): Observable<never> {
    return throwError(() => error);
  }
}
