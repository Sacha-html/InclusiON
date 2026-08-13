import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import {
  ApiResponse,
  AssignInstitutionRequest,
  AssignPersonRequest,
  ProfessionalInstitutionResponse,
  ProfessionalPersonResponse,
  CreateClassroomRequest,
  ClassroomResponse,
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

  movePersonToClassroom(profId: string, personId: string, classroomId: string | null): Observable<ProfessionalPersonResponse> {
    return this.http
      .put<ApiResponse<ProfessionalPersonResponse>>(`${this.professionalsUrl(profId)}/persons/${personId}/classroom`, { classroomId })
      .pipe(unwrapResponse());
  }

  createClassroom(profId: string, request: CreateClassroomRequest): Observable<ProfessionalPersonResponse[]> {
    return this.http
      .post<ApiResponse<ProfessionalPersonResponse[]>>(`${this.professionalsUrl(profId)}/classroom`, request)
      .pipe(unwrapResponse());
  }

  getClassroomsByProfessional(profId: string): Observable<ClassroomResponse[]> {
    return this.http
      .get<ApiResponse<ClassroomResponse[]>>(`${this.professionalsUrl(profId)}/classrooms`)
      .pipe(unwrapResponse());
  }

  updateClassroom(profId: string, classroomId: string, name: string): Observable<ClassroomResponse> {
    return this.http
      .put<ApiResponse<ClassroomResponse>>(`${this.professionalsUrl(profId)}/classrooms/${classroomId}`, { name })
      .pipe(unwrapResponse());
  }

  deactivateClassroom(profId: string, classroomId: string): Observable<ClassroomResponse> {
    return this.http
      .put<ApiResponse<ClassroomResponse>>(`${this.professionalsUrl(profId)}/classrooms/${classroomId}/deactivate`, {})
      .pipe(unwrapResponse());
  }

  deleteClassroom(profId: string, classroomId: string): Observable<ClassroomResponse> {
    return this.http
      .delete<ApiResponse<ClassroomResponse>>(`${this.professionalsUrl(profId)}/classrooms/${classroomId}`)
      .pipe(unwrapResponse());
  }

  deactivatePersonAssignment(profId: string, personId: string): Observable<void> {
    return this.http
      .put<void>(`${this.professionalsUrl(profId)}/persons/${personId}/deactivate`, {})
      .pipe(handleApiError());
  }

  transferStudent(request: { personId: string; fromProfessionalId: string; toProfessionalId: string }): Observable<any> {
    return this.http
      .post<ApiResponse<any>>(`${environment.apiUrl}/Professionals/transfer-student`, request)
      .pipe(unwrapResponse());
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
