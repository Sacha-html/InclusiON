import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, catchError, Observable, of, tap, throwError } from 'rxjs';
import { map } from 'rxjs/operators';
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

@Injectable({
  providedIn: 'root',
})
export class CatalogsService {
  private http = inject(HttpClient);

  private get apiUrl(): string {
    return `${environment.apiUrl}/Catalogs`;
  }

  // Caches
  private disabilityTypes$ = new BehaviorSubject<CatalogItem[] | null>(null);
  private autonomyLevels$ = new BehaviorSubject<AutonomyLevelItem[] | null>(null);
  private loginMethods$ = new BehaviorSubject<LoginMethodItem[] | null>(null);
  private activityCategories$ = new BehaviorSubject<ActivityCategoryItem[] | null>(null);
  private skillAreas$ = new BehaviorSubject<SkillAreaItem[] | null>(null);
  private activityTemplateTypes$ = new BehaviorSubject<ActivityTemplateTypeItem[] | null>(null);

  getDisabilityTypes(): Observable<CatalogItem[]> {
    if (this.disabilityTypes$.value) {
      return of(this.disabilityTypes$.value);
    }
    return this.http
      .get<ApiResponse<CatalogItem[]>>(`${this.apiUrl}/disability-types`)
      .pipe(
        map(response => response.data),
        tap(data => this.disabilityTypes$.next(data)),
        catchError(this.handleError),
      );
  }

  getAutonomyLevels(): Observable<AutonomyLevelItem[]> {
    if (this.autonomyLevels$.value) {
      return of(this.autonomyLevels$.value);
    }
    return this.http
      .get<ApiResponse<AutonomyLevelItem[]>>(`${this.apiUrl}/autonomy-levels`)
      .pipe(
        map(response => response.data),
        tap(data => this.autonomyLevels$.next(data)),
        catchError(this.handleError),
      );
  }

  getLoginMethods(): Observable<LoginMethodItem[]> {
    if (this.loginMethods$.value) {
      return of(this.loginMethods$.value);
    }
    return this.http
      .get<ApiResponse<LoginMethodItem[]>>(`${this.apiUrl}/login-methods`)
      .pipe(
        map(response => response.data),
        tap(data => this.loginMethods$.next(data)),
        catchError(this.handleError),
      );
  }

  getActivityCategories(): Observable<ActivityCategoryItem[]> {
    if (this.activityCategories$.value) {
      return of(this.activityCategories$.value);
    }
    return this.http
      .get<ApiResponse<ActivityCategoryItem[]>>(`${this.apiUrl}/activity-categories`)
      .pipe(
        map(response => response.data),
        tap(data => this.activityCategories$.next(data)),
        catchError(this.handleError),
      );
  }

  getSkillAreas(): Observable<SkillAreaItem[]> {
    if (this.skillAreas$.value) {
      return of(this.skillAreas$.value);
    }
    return this.http
      .get<ApiResponse<SkillAreaItem[]>>(`${this.apiUrl}/skill-areas`)
      .pipe(
        map(response => response.data),
        tap(data => this.skillAreas$.next(data)),
        catchError(this.handleError),
      );
  }

  getActivityTemplateTypes(): Observable<ActivityTemplateTypeItem[]> {
    if (this.activityTemplateTypes$.value) {
      return of(this.activityTemplateTypes$.value);
    }
    return this.http
      .get<ApiResponse<ActivityTemplateTypeItem[]>>(`${this.apiUrl}/activity-template-types`)
      .pipe(
        map(response => response.data),
        tap(data => this.activityTemplateTypes$.next(data)),
        catchError(this.handleError),
      );
  }

  clearCache(): void {
    this.disabilityTypes$.next(null);
    this.autonomyLevels$.next(null);
    this.loginMethods$.next(null);
    this.activityCategories$.next(null);
    this.skillAreas$.next(null);
    this.activityTemplateTypes$.next(null);
  }

  private handleError(error: unknown): Observable<never> {
    return throwError(() => error);
  }
}
