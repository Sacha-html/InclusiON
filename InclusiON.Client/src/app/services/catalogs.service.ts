import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, shareReplay, Subject, startWith, switchMap } from 'rxjs';
import {
  ApiResponse,
  CatalogItem,
  AutonomyLevelItem,
  LoginMethodItem,
  ActivityCategoryItem,
  SkillAreaItem,
  ActivityTemplateTypeItem,
  AvatarColorItem,
  SpecialtyItem,
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
  private readonly specialtiesChangedSubject = new Subject<void>();
  private readonly disabilityTypesChangedSubject = new Subject<void>();
  private readonly autonomyLevelsChangedSubject = new Subject<void>();
  private readonly skillAreasChangedSubject = new Subject<void>();
  private readonly activityTemplateTypesChangedSubject = new Subject<void>();
  readonly specialtiesChanged$ = this.specialtiesChangedSubject.asObservable();
  private readonly disabilityTypes$ = this.disabilityTypesChangedSubject.pipe(
    startWith(void 0),
    switchMap(() => this.http
      .get<ApiResponse<CatalogItem[]>>(`${this.apiUrl}/disability-types`)
      .pipe(unwrapResponse())),
    shareReplay({ bufferSize: 1, refCount: true }),
  );
  private readonly autonomyLevels$ = this.autonomyLevelsChangedSubject.pipe(
    startWith(void 0),
    switchMap(() => this.http
      .get<ApiResponse<AutonomyLevelItem[]>>(`${this.apiUrl}/autonomy-levels`)
      .pipe(unwrapResponse())),
    shareReplay({ bufferSize: 1, refCount: true }),
  );
  private readonly skillAreas$ = this.skillAreasChangedSubject.pipe(
    startWith(void 0),
    switchMap(() => this.http
      .get<ApiResponse<SkillAreaItem[]>>(`${this.apiUrl}/skill-areas`)
      .pipe(unwrapResponse())),
    shareReplay({ bufferSize: 1, refCount: true }),
  );
  private readonly specialties$ = this.specialtiesChangedSubject.pipe(
    startWith(void 0),
    switchMap(() => this.http
      .get<ApiResponse<SpecialtyItem[]>>(`${this.apiUrl}/specialties`)
      .pipe(unwrapResponse())),
    shareReplay({ bufferSize: 1, refCount: true }),
  );
  private readonly activityTemplateTypes$ = this.activityTemplateTypesChangedSubject.pipe(
    startWith(void 0),
    switchMap(() => this.http
      .get<ApiResponse<ActivityTemplateTypeItem[]>>(`${this.apiUrl}/activity-template-types`)
      .pipe(unwrapResponse())),
    shareReplay({ bufferSize: 1, refCount: true }),
  );

  getDisabilityTypes(): Observable<CatalogItem[]> {
    return this.disabilityTypes$;
  }

  invalidateDisabilityTypes(): void {
    this.disabilityTypesChangedSubject.next();
  }

  getSpecialties(): Observable<SpecialtyItem[]> { return this.specialties$; }

  invalidateSpecialties(): void {
    this.specialtiesChangedSubject.next();
  }

  getAutonomyLevels(): Observable<AutonomyLevelItem[]> {
    return this.autonomyLevels$;
  }

  invalidateAutonomyLevels(): void {
    this.autonomyLevelsChangedSubject.next();
  }

  getLoginMethods(): Observable<LoginMethodItem[]> {
    return this.cached('login-methods');
  }

  getActivityCategories(): Observable<ActivityCategoryItem[]> {
    return this.cached('activity-categories');
  }

  getSkillAreas(): Observable<SkillAreaItem[]> {
    return this.skillAreas$;
  }

  invalidateSkillAreas(): void {
    this.skillAreasChangedSubject.next();
  }

  getActivityTemplateTypes(): Observable<ActivityTemplateTypeItem[]> {
    return this.activityTemplateTypes$;
  }

  invalidateActivityTemplateTypes(): void {
    this.activityTemplateTypesChangedSubject.next();
  }

  getAvatarColors(): Observable<AvatarColorItem[]> {
    return this.cached('avatar-colors');
  }

  getReportTypes(): Observable<CatalogItem[]> {
    return this.cached('report-types');
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
