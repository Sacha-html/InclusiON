import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import { ApiResponse, RoleResponse } from '@models';
import { Observable } from 'rxjs';
import { unwrapResponse } from '@shared/utils';

@Injectable({
  providedIn: 'root',
})
export class RolesService {
  private readonly http = inject(HttpClient);

  private get apiUrl(): string {
    return `${environment.apiUrl}/Roles`;
  }

  getRoles(): Observable<RoleResponse[]> {
    return this.http
      .get<ApiResponse<RoleResponse[]>>(this.apiUrl)
      .pipe(unwrapResponse());
  }

  getRoleById(id: string): Observable<RoleResponse> {
    return this.http
      .get<ApiResponse<RoleResponse>>(`${this.apiUrl}/${id}`)
      .pipe(unwrapResponse());
  }

  getAvailablePermissions(): Observable<string[]> {
    return this.http
      .get<ApiResponse<string[]>>(`${this.apiUrl}/available-permissions`)
      .pipe(unwrapResponse());
  }

  updateRolePermissions(roleId: string, permissions: string[]): Observable<RoleResponse> {
    return this.http
      .put<ApiResponse<RoleResponse>>(`${this.apiUrl}/${roleId}/permissions`, { permissions })
      .pipe(unwrapResponse());
  }
}
