import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '@env';
import { unwrapResponse } from '@shared/utils';
import { ApiResponse, RoadmapResponse, RoadmapAreaResponse, RoadmapActivityResponse, AddRoadmapActivityRequest, ReorderActivityItem, AdaptiveAdjustmentLogResponse, SkillRadarPointResponse, AdaptiveEngineConfigResponse, ActivityAssignmentResponse } from '@models';

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

  getAdjustmentHistory(personId: string, areaId: number, activityEntryId: number): Observable<AdaptiveAdjustmentLogResponse[]> {
    return this.http
      .get<ApiResponse<AdaptiveAdjustmentLogResponse[]>>(
        `${this.url(personId)}/areas/${areaId}/activities/${activityEntryId}/adjustments`
      )
      .pipe(map(r => r.data ?? []));
  }

  getSkillRadar(personId: string): Observable<SkillRadarPointResponse[]> {
    return this.http
      .get<ApiResponse<SkillRadarPointResponse[]>>(`${this.url(personId)}/skill-radar`)
      .pipe(map(r => r.data ?? []));
  }

  // ── Adaptive Engine Config (IN-116) ──────────────────────────────────────

  getAdaptiveConfig(personId: string, areaId: number, activityEntryId: number): Observable<AdaptiveEngineConfigResponse | null> {
    return this.http
      .get<ApiResponse<AdaptiveEngineConfigResponse | null>>(
        `${this.url(personId)}/areas/${areaId}/activities/${activityEntryId}/adaptive-config`
      )
      .pipe(map(r => r.data ?? null));
  }

  upsertAdaptiveConfig(
    personId: string,
    areaId: number,
    activityEntryId: number,
    payload: Partial<AdaptiveEngineConfigResponse>
  ): Observable<AdaptiveEngineConfigResponse> {
    return this.http
      .put<ApiResponse<AdaptiveEngineConfigResponse>>(
        `${this.url(personId)}/areas/${areaId}/activities/${activityEntryId}/adaptive-config`,
        payload
      )
      .pipe(unwrapResponse());
  }

  // ── Assign from Roadmap (IN-150) ─────────────────────────────────────────

  assignFromRoadmap(
    personId: string,
    areaId: number,
    activityEntryId: number,
    payload: { dueDate?: string; isEvaluationActivity: boolean }
  ): Observable<ActivityAssignmentResponse> {
    return this.http
      .post<ApiResponse<ActivityAssignmentResponse>>(
        `${this.url(personId)}/areas/${areaId}/activities/${activityEntryId}/assign`,
        payload
      )
      .pipe(unwrapResponse());
  }

  deleteAdaptiveConfig(personId: string, areaId: number, activityEntryId: number): Observable<unknown> {
    return this.http
      .delete<ApiResponse<unknown>>(
        `${this.url(personId)}/areas/${areaId}/activities/${activityEntryId}/adaptive-config`
      )
      .pipe(map(r => r));
  }
}
