import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, shareReplay, Subject, tap } from 'rxjs';
import {
  ApiResponse,
  CatalogItem,
  AutonomyLevelItem,
  LoginMethodItem,
  ActivityCategoryItem,
  SkillAreaItem,
  ActivityTemplateTypeItem,
} from '@models';
import { environment } from '@env';
import { unwrapResponse } from '@shared/utils';

@Injectable({
  providedIn: 'root',
})
export class CatalogsService {
  private readonly http = inject(HttpClient);

  private get apiUrl(): string {
    return `${environment.apiUrl}/Catalogs`;
  }

  private cache = new Map<string, Observable<any>>();
  private clearCache$ = new Subject<void>();

  getDisabilityTypes(): Observable<CatalogItem[]> {
    return this.cached('disability-types');
  }

  getAutonomyLevels(): Observable<AutonomyLevelItem[]> {
    return this.cached('autonomy-levels');
  }

  getLoginMethods(): Observable<LoginMethodItem[]> {
    return this.cached('login-methods');
  }

  getActivityCategories(): Observable<ActivityCategoryItem[]> {
    return this.cached('activity-categories');
  }

  getSkillAreas(): Observable<SkillAreaItem[]> {
    return this.cached('skill-areas');
  }

  getActivityTemplateTypes(): Observable<ActivityTemplateTypeItem[]> {
    return this.cached('activity-template-types');
  }

  clearCache(): void {
    this.cache.clear();
    this.clearCache$.next();
  }

  private cached<T>(endpoint: string): Observable<T> {
    if (!this.cache.has(endpoint)) {
      this.cache.set(
        endpoint,
        this.http
          .get<ApiResponse<T>>(`${this.apiUrl}/${endpoint}`)
          .pipe(
            unwrapResponse(),
            shareReplay({ bufferSize: 1, refCount: false }),
          ),
      );
    }
    return this.cache.get(endpoint)!;
  }
}
