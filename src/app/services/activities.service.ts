import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '@env';
import { unwrapResponse } from '@shared/utils';
import { ApiResponse, PagedResponse } from '@models';
import {
  ActivityListItemResponse,
  ActivityResponse,
  ActivityAssignmentResponse,
} from '@models/responses/activity.response';
import {
  GetActivitiesRequest,
  CreateActivityRequest,
  UpdateActivityRequest,
  CreateAssignmentRequest,
} from '@models/requests/activities';

@Injectable({ providedIn: 'root' })
export class ActivitiesService {
  private readonly http = inject(HttpClient);

  private get baseUrl(): string {
    return `${environment.apiUrl}/Activities`;
  }

  private get assignmentsUrl(): string {
    return `${environment.apiUrl}/activity-assignments`;
  }

  getActivities(request: GetActivitiesRequest): Observable<PagedResponse<ActivityListItemResponse>> {
    let params = new HttpParams();
    if (request.page)           params = params.set('page', request.page.toString());
    if (request.pageSize)       params = params.set('pageSize', request.pageSize.toString());
    if (request.search)         params = params.set('search', request.search);
    if (request.categoryId)     params = params.set('categoryId', request.categoryId.toString());
    if (request.skillAreaId)    params = params.set('skillAreaId', request.skillAreaId.toString());
    if (request.templateTypeId) params = params.set('templateTypeId', request.templateTypeId.toString());
    if (request.isActive !== undefined && request.isActive !== null)
      params = params.set('isActive', request.isActive.toString());
    if (request.isStandard !== undefined && request.isStandard !== null)
      params = params.set('isStandard', request.isStandard.toString());
    if (request.sortBy)         params = params.set('sortBy', request.sortBy);
    if (request.sortDirection)  params = params.set('sortDirection', request.sortDirection);

    return this.http
      .get<ApiResponse<PagedResponse<ActivityListItemResponse>>>(this.baseUrl, { params })
      .pipe(unwrapResponse());
  }

  getById(id: string): Observable<ActivityResponse> {
    return this.http
      .get<ApiResponse<ActivityResponse>>(`${this.baseUrl}/${id}`)
      .pipe(unwrapResponse());
  }

  create(request: CreateActivityRequest): Observable<ActivityResponse> {
    return this.http
      .post<ApiResponse<ActivityResponse>>(this.baseUrl, request)
      .pipe(unwrapResponse());
  }

  update(id: string, request: UpdateActivityRequest): Observable<ActivityResponse> {
    return this.http
      .put<ApiResponse<ActivityResponse>>(`${this.baseUrl}/${id}`, request)
      .pipe(unwrapResponse());
  }

  setStatus(id: string, isActive: boolean): Observable<ActivityResponse> {
    return this.http
      .patch<ApiResponse<ActivityResponse>>(`${this.baseUrl}/${id}`, { isActive })
      .pipe(unwrapResponse());
  }

  createAssignment(request: CreateAssignmentRequest): Observable<ActivityAssignmentResponse> {
    return this.http
      .post<ApiResponse<ActivityAssignmentResponse>>(this.assignmentsUrl, request)
      .pipe(unwrapResponse());
  }

  searchSemantic(text: string, limit = 10): Observable<ActivityListItemResponse[]> {
    const params = new HttpParams()
      .set('text', text)
      .set('limit', limit.toString());
    return this.http
      .get<ApiResponse<ActivityListItemResponse[]>>(`${this.baseUrl}/search`, { params })
      .pipe(unwrapResponse());
  }

  getPersonAssignments(personId: string): Observable<ActivityAssignmentResponse[]> {
    return this.http
      .get<ApiResponse<ActivityAssignmentResponse[]>>(
        `${environment.apiUrl}/persons/${personId}/activity-assignments`
      )
      .pipe(unwrapResponse());
  }

  getMyAssignments(): Observable<ActivityAssignmentResponse[]> {
    return this.http
      .get<ApiResponse<ActivityAssignmentResponse[]>>(
        `${environment.apiUrl}/my/activity-assignments`
      )
      .pipe(unwrapResponse());
  }

  startResponse(assignmentId: string): Observable<ActivityAssignmentResponse> {
    return this.http
      .post<ApiResponse<ActivityAssignmentResponse>>(
        `${this.assignmentsUrl}/${assignmentId}/responses/start`,
        {}
      )
      .pipe(unwrapResponse());
  }

  completeResponse(
    assignmentId: string,
    responseId: string,
    data: { successPercentage: number; timeSpentSeconds: number; requiredSupport: boolean; frustrationLevel?: number; responsePattern?: string; observations?: string }
  ): Observable<ActivityAssignmentResponse> {
    return this.http
      .post<ApiResponse<ActivityAssignmentResponse>>(
        `${this.assignmentsUrl}/${assignmentId}/responses/${responseId}/complete`,
        data
      )
      .pipe(unwrapResponse());
  }
}
