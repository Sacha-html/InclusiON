import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import { AdminUserResponse, ApiResponse, CreateAdminUserResponse } from '../models';
import { Observable } from 'rxjs';
import { unwrapResponse, handleApiError } from '@shared/utils';

export interface UpdateAdminUserRequest {
  name: string;
  surname: string;
  email: string;
}

@Injectable({
  providedIn: 'root',
})
export class AdminUsersService {
  private readonly http = inject(HttpClient);

  private get apiUrl(): string {
    return `${environment.apiUrl}/admin/institutions-assignments`;
  }

  private get usersApiUrl(): string {
    return `${environment.apiUrl}/admin/users`;
  }

  getAdmins(): Observable<AdminUserResponse[]> {
    return this.http
      .get<ApiResponse<AdminUserResponse[]>>(`${this.apiUrl}/admins`)
      .pipe(unwrapResponse());
  }

  createAdmin(request: {
    email: string;
    firstName: string;
    lastName: string;
    institutionId: number;
  }): Observable<CreateAdminUserResponse> {
    return this.http
      .post<ApiResponse<CreateAdminUserResponse>>(`${this.apiUrl}/users`, request)
      .pipe(unwrapResponse());
  }

  updateAdmin(userId: string, request: UpdateAdminUserRequest): Observable<void> {
    return this.http
      .put<void>(`${this.usersApiUrl}/${userId}`, request)
      .pipe(handleApiError());
  }
}
