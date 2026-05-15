import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import { AdminUserResponse, AdminDashboardResponse, ApiResponse, CreateAdminUserResponse, PagedResponse } from '@models';
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

  getAdmins(page = 1, pageSize = 10, search?: string): Observable<PagedResponse<AdminUserResponse>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    if (search) params = params.set('search', search);

    return this.http
      .get<ApiResponse<PagedResponse<AdminUserResponse>>>(`${this.apiUrl}/admins`, { params })
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

  getDashboard(): Observable<AdminDashboardResponse> {
    return this.http
      .get<ApiResponse<AdminDashboardResponse>>(`${environment.apiUrl}/admin/dashboard`)
      .pipe(unwrapResponse());
  }
}
