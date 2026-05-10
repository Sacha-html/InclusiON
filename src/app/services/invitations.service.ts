import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import {
  ApiResponse,
  CreateInvitationRequest,
  AcceptInvitationRequest,
  InvitationResponse,
  InvitationValidationResponse,
  AcceptInvitationResponse,
  PagedResponse,
} from '../models';
import { Observable } from 'rxjs';
import { unwrapResponse } from '@shared/utils';

@Injectable({
  providedIn: 'root',
})
export class InvitationsService {
  private readonly http = inject(HttpClient);

  private get apiUrl(): string {
    return `${environment.apiUrl}/Invitations`;
  }

  getAll(page = 1, pageSize = 10): Observable<PagedResponse<InvitationResponse>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http
      .get<ApiResponse<PagedResponse<InvitationResponse>>>(this.apiUrl, { params })
      .pipe(unwrapResponse());
  }

  create(request: CreateInvitationRequest): Observable<InvitationResponse> {
    return this.http
      .post<ApiResponse<InvitationResponse>>(this.apiUrl, request)
      .pipe(unwrapResponse());
  }

  validateCode(code: string): Observable<InvitationValidationResponse> {
    return this.http
      .get<ApiResponse<InvitationValidationResponse>>(`${this.apiUrl}/${code}`)
      .pipe(unwrapResponse());
  }

  accept(code: string, request: AcceptInvitationRequest): Observable<AcceptInvitationResponse> {
    return this.http
      .post<ApiResponse<AcceptInvitationResponse>>(`${this.apiUrl}/${code}/accept`, request)
      .pipe(unwrapResponse());
  }
}
