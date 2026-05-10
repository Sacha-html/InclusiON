import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiResponse, PagedResponse } from '../models';
import { AdminUserListItemResponse } from '../models/responses/admin-user-list-item.response';
import { AdminUserDetailResponse, ResetPasswordResultResponse, UserRecentSessionResponse } from '../models/responses/admin-user-detail.response';
import { GetAdminUsersRequest } from '../models/requests/admin-users/get-admin-users.request';
import { unwrapResponse, handleApiError } from '@shared/utils';

@Injectable({
  providedIn: 'root',
})
export class UserManagementService {
  private readonly http = inject(HttpClient);

  private get apiUrl(): string {
    return `${environment.apiUrl}/admin/users`;
  }

  getUsers(request?: GetAdminUsersRequest): Observable<PagedResponse<AdminUserListItemResponse>> {
    const params = this.buildGetUsersParams(request);
    return this.http
      .get<ApiResponse<PagedResponse<AdminUserListItemResponse>>>(this.apiUrl, { params })
      .pipe(unwrapResponse());
  }

  private buildGetUsersParams(request?: GetAdminUsersRequest): HttpParams {
    let params = new HttpParams();
    if (!request) return params;
    if (request.page)                    params = params.set('page',          request.page.toString());
    if (request.pageSize)                params = params.set('pageSize',       request.pageSize.toString());
    if (request.search)                  params = params.set('search',         request.search);
    if (request.role)                    params = params.set('role',           request.role);
    if (request.isActive !== undefined)  params = params.set('isActive',       request.isActive.toString());
    if (request.institutionId)           params = params.set('institutionId',  request.institutionId.toString());
    if (request.sortBy)                  params = params.set('sortBy',         request.sortBy);
    if (request.sortDirection)           params = params.set('sortDirection',  request.sortDirection);
    return params;
  }

  getUserDetail(userId: string): Observable<AdminUserDetailResponse> {
    return this.http
      .get<ApiResponse<AdminUserDetailResponse>>(`${this.apiUrl}/${userId}`)
      .pipe(unwrapResponse());
  }

  resetPassword(userId: string): Observable<ResetPasswordResultResponse> {
    return this.http
      .post<ApiResponse<ResetPasswordResultResponse>>(`${this.apiUrl}/${userId}/reset-password`, {})
      .pipe(unwrapResponse());
  }

  deactivateUser(userId: string): Observable<ApiResponse<void>> {
    return this.http
      .put<ApiResponse<void>>(`${this.apiUrl}/${userId}/deactivate`, {})
      .pipe(handleApiError());
  }

  reactivateUser(userId: string): Observable<ResetPasswordResultResponse> {
    return this.http
      .put<ApiResponse<ResetPasswordResultResponse>>(`${this.apiUrl}/${userId}/reactivate`, {})
      .pipe(unwrapResponse());
  }

  getUserActivity(userId: string, page = 1, pageSize = 15): Observable<UserRecentSessionResponse[]> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http
      .get<ApiResponse<PagedResponse<UserRecentSessionResponse>>>(`${this.apiUrl}/${userId}/activity`, { params })
      .pipe(unwrapResponse(), map((r) => r.data));
  }
}
