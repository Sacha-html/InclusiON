import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '@env';
import { unwrapResponse } from '@shared/utils';
import { ApiResponse } from '@models';
import { RoadmapResponse, RoadmapAreaResponse, RoadmapActivityResponse } from '@models/responses/roadmap.response';
import { AddRoadmapActivityRequest, ReorderActivityItem } from '@models/requests/roadmap';

@Injectable({ providedIn: 'root' })
export class RoadmapService {
  private readonly http = inject(HttpClient);

  private url(personId: string): string {
    return `${environment.apiUrl}/Persons/${personId}/roadmap`;
  }

  getRoadmap(personId: string): Observable<RoadmapResponse> {
    return this.http
      .get<ApiResponse<RoadmapResponse>>(this.url(personId))
      .pipe(unwrapResponse());
  }

  getMyRoadmap(): Observable<RoadmapResponse> {
    return this.http
      .get<ApiResponse<RoadmapResponse>>(`${environment.apiUrl}/my/roadmap`)
      .pipe(unwrapResponse());
  }

  createRoadmap(personId: string, notes?: string | null): Observable<RoadmapResponse> {
    return this.http
      .post<ApiResponse<RoadmapResponse>>(this.url(personId), { notes })
      .pipe(unwrapResponse());
  }

  updateNotes(personId: string, notes: string | null): Observable<RoadmapResponse> {
    return this.http
      .patch<ApiResponse<RoadmapResponse>>(`${this.url(personId)}/notes`, { notes })
      .pipe(unwrapResponse());
  }

  addArea(personId: string, skillAreaId: number, displayOrder: number): Observable<RoadmapAreaResponse> {
    return this.http
      .post<ApiResponse<RoadmapAreaResponse>>(`${this.url(personId)}/areas`, { skillAreaId, displayOrder })
      .pipe(unwrapResponse());
  }

  removeArea(personId: string, areaId: string): Observable<unknown> {
    return this.http
      .delete<ApiResponse<unknown>>(`${this.url(personId)}/areas/${areaId}`)
      .pipe(unwrapResponse());
  }

  addActivity(personId: string, areaId: string, request: AddRoadmapActivityRequest): Observable<RoadmapActivityResponse> {
    return this.http
      .post<ApiResponse<RoadmapActivityResponse>>(`${this.url(personId)}/areas/${areaId}/activities`, request)
      .pipe(unwrapResponse());
  }

  removeActivity(personId: string, areaId: string, activityEntryId: string): Observable<unknown> {
    return this.http
      .delete<ApiResponse<unknown>>(`${this.url(personId)}/areas/${areaId}/activities/${activityEntryId}`)
      .pipe(unwrapResponse());
  }

  reorderActivities(personId: string, areaId: string, activities: ReorderActivityItem[]): Observable<unknown> {
    return this.http
      .put<ApiResponse<unknown>>(
        `${this.url(personId)}/areas/${areaId}/activities/reorder`,
        { activities }
      )
      .pipe(unwrapResponse());
  }

  unlockActivity(personId: string, areaId: string, activityEntryId: string): Observable<RoadmapActivityResponse> {
    return this.http
      .put<ApiResponse<RoadmapActivityResponse>>(
        `${this.url(personId)}/areas/${areaId}/activities/${activityEntryId}/unlock`,
        {}
      )
      .pipe(unwrapResponse());
  }
}
