import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import {
  ApiResponse,
  CreateInvitationRequest,
  AcceptInvitationRequest,
  InvitationResponse,
  InvitationValidationResponse,
  AcceptInvitationResponse,
} from '../models';
import { catchError, map, Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class InvitationsService {
  private readonly http = inject(HttpClient);

  private get apiUrl(): string {
    return `${environment.apiUrl}/Invitations`;
  }

  getAll(): Observable<InvitationResponse[]> {
    return this.http
      .get<ApiResponse<InvitationResponse[]>>(this.apiUrl)
      .pipe(
        map((response) => response.data),
        catchError(this.handleError),
      );
  }

  create(request: CreateInvitationRequest): Observable<InvitationResponse> {
    return this.http
      .post<ApiResponse<InvitationResponse>>(this.apiUrl, request)
      .pipe(
        map((response) => response.data),
        catchError(this.handleError),
      );
  }

  validateCode(code: string): Observable<InvitationValidationResponse> {
    return this.http
      .get<ApiResponse<InvitationValidationResponse>>(`${this.apiUrl}/${code}`)
      .pipe(
        map((response) => response.data),
        catchError(this.handleError),
      );
  }

  accept(code: string, request: AcceptInvitationRequest): Observable<AcceptInvitationResponse> {
    return this.http
      .post<ApiResponse<AcceptInvitationResponse>>(`${this.apiUrl}/${code}/accept`, request)
      .pipe(
        map((response) => response.data),
        catchError(this.handleError),
      );
  }

  private handleError(error: unknown): Observable<never> {
    return throwError(() => error);
  }
}
