import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import {
  ApiResponse,
  PagedResponse,
  PersonResponse,
  PersonListItemResponse,
  PersonSkillProfileResponse,
  CreatePersonRequest,
  UpdatePersonRequest,
  GetPersonsRequest,
  ProfessionalPersonResponse,
  UpdateLoginMethodRequest,
  UpdateLoginMethodResponse,
  SupervisorCandidate,
} from '@models';
import { environment } from '@env';
import { unwrapResponse, handleApiError } from '@shared/utils';

@Injectable({
  providedIn: 'root',
})
export class PersonsService {
  private readonly http = inject(HttpClient);

  private get apiUrl(): string {
    return `${environment.apiUrl}/Persons`;
  }

  /**
   * Obtiene la lista paginada de personas con filtros opcionales.
   * Requiere rol ProfessionalOrAbove.
   */
  getPersons(
    request?: GetPersonsRequest
  ): Observable<PagedResponse<PersonListItemResponse>> {
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
      if (request.institutionId) {
        params = params.set('institutionId', request.institutionId.toString());
      }
      if (request.representativeSearch) {
        params = params.set('representativeSearch', request.representativeSearch);
      }
    }

    return this.http
      .get<ApiResponse<PagedResponse<PersonListItemResponse>>>(this.apiUrl, {
        params,
      })
      .pipe(unwrapResponse());
  }

  /**
   * Obtiene una persona por su ID.
   * Requiere rol ValidUser.
   */
  getPersonById(personId: string): Observable<PersonResponse> {
    return this.http
      .get<ApiResponse<PersonResponse>>(`${this.apiUrl}/${personId}`)
      .pipe(unwrapResponse());
  }

  /**
   * Obtiene el perfil de la persona autenticada.
   * Requiere autenticación.
   */
  getMyProfile(): Observable<PersonResponse> {
    return this.http
      .get<ApiResponse<PersonResponse>>(`${this.apiUrl}/me`)
      .pipe(unwrapResponse());
  }

  /**
   * Crea una nueva persona con discapacidad.
   * Requiere rol ProfessionalOrAbove.
   */
  createPerson(
    request: CreatePersonRequest
  ): Observable<PersonResponse> {
    return this.http
      .post<ApiResponse<PersonResponse>>(this.apiUrl, request)
      .pipe(unwrapResponse());
  }

  /**
   * Actualiza una persona existente.
   * Requiere rol ProfessionalOrAbove.
   */
  updatePerson(
    personId: string,
    request: UpdatePersonRequest
  ): Observable<PersonResponse> {
    return this.http
      .put<ApiResponse<PersonResponse>>(`${this.apiUrl}/${personId}`, request)
      .pipe(unwrapResponse());
  }

  /**
   * Actualiza el perfil de la persona autenticada.
   * Requiere autenticación.
   */
  deactivatePerson(personId: string): Observable<PersonResponse> {
    return this.http
      .put<ApiResponse<PersonResponse>>(`${this.apiUrl}/${personId}/deactivate`, {})
      .pipe(unwrapResponse());
  }

  updateMyProfile(
    request: UpdatePersonRequest
  ): Observable<PersonResponse> {
    return this.http
      .put<ApiResponse<PersonResponse>>(`${this.apiUrl}/me`, request)
      .pipe(unwrapResponse());
  }

  /**
   * Obtiene el perfil de habilidades de una persona.
   */
  getSkillProfile(
    personId: string,
    all?: boolean
  ): Observable<PersonSkillProfileResponse[]> {
    let params = new HttpParams();
    if (all) {
      params = params.set('all', 'true');
    }
    return this.http
      .get<ApiResponse<PersonSkillProfileResponse[]>>(
        `${this.apiUrl}/${personId}/skill-profile`,
        { params }
      )
      .pipe(unwrapResponse());
  }

  /**
   * Asigna un area de habilidad a una persona.
   */
  addSkillArea(
    personId: string,
    skillAreaId: number
  ): Observable<PersonSkillProfileResponse> {
    return this.http
      .post<ApiResponse<PersonSkillProfileResponse>>(
        `${this.apiUrl}/${personId}/skill-profile`,
        { skillAreaId }
      )
      .pipe(unwrapResponse());
  }

  /**
   * Desactiva un area de habilidad de una persona.
   */
  deactivateSkillArea(
    personId: string,
    areaId: number
  ): Observable<PersonSkillProfileResponse> {
    return this.http
      .put<ApiResponse<PersonSkillProfileResponse>>(
        `${this.apiUrl}/${personId}/skill-profile/${areaId}`,
        {}
      )
      .pipe(unwrapResponse());
  }

  /**
   * Obtiene los profesionales asignados a una persona.
   */
  getProfessionalsByPerson(personId: string): Observable<ProfessionalPersonResponse[]> {
    return this.http
      .get<ApiResponse<ProfessionalPersonResponse[]>>(`${this.apiUrl}/${personId}/professionals`)
      .pipe(unwrapResponse());
  }

  /**
   * Lista candidatos a supervisor (profesionales asignados + familiares vinculados).
   */
  getSupervisorCandidates(personId: string, page = 1, pageSize = 50): Observable<SupervisorCandidate[]> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http
      .get<ApiResponse<PagedResponse<SupervisorCandidate>>>(`${this.apiUrl}/${personId}/supervisor-candidates`, { params })
      .pipe(unwrapResponse(), map((r) => r.data));
  }

  /**
   * Actualiza solo la configuración de accesibilidad de una persona.
   */
  updateAccessibilityConfig(
    personId: string,
    config: {
      requiresLargeFont: boolean;
      requiresHighContrast: boolean;
      visualNoiseSensitivity: boolean;
      soundSensitivity: boolean;
      colorBlindnessType: string;
    }
  ): Observable<PersonResponse> {
    return this.http
      .put<ApiResponse<PersonResponse>>(`${this.apiUrl}/${personId}`, config)
      .pipe(unwrapResponse());
  }

  /**
   * Cambia el método de login de una persona. Si el método nuevo es STANDARD,
   * la respuesta incluye una contraseña temporal de un solo uso.
   */
  updateLoginMethod(userId: string, request: UpdateLoginMethodRequest): Observable<UpdateLoginMethodResponse> {
    return this.http
      .put<ApiResponse<UpdateLoginMethodResponse>>(`${this.apiUrl}/${userId}/login-method`, request)
      .pipe(unwrapResponse());
  }
}
