import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { ApiResponse, AnalyticsDashboardResponse } from '@models';

@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Analytics`;

  /**
   * Obtiene las métricas analíticas y KPIs para el profesional autenticado.
   * Si se especifica aulaId, filtra los datos exclusivamente para esa clase.
   * Permite filtrar por fecha de corte (desde / hasta).
   */
  getProfessionalAnalytics(aulaId?: string | null, desde?: string | null, hasta?: string | null): Observable<AnalyticsDashboardResponse> {
    let params = new HttpParams();
    if (aulaId && aulaId.trim().length > 0) {
      params = params.set('aulaId', aulaId);
    }
    if (desde && desde.trim().length > 0) {
      params = params.set('desde', desde.trim()).set('dateFrom', desde.trim());
    }
    if (hasta && hasta.trim().length > 0) {
      params = params.set('hasta', hasta.trim()).set('dateTo', hasta.trim());
    }
    return this.http
      .get<ApiResponse<AnalyticsDashboardResponse>>(`${this.baseUrl}/professional`, { params })
      .pipe(map((res) => res.data));
  }

  /**
   * Obtiene las métricas analíticas globales de toda la institución para el panel Administrador.
   * Permite filtrar por fecha de corte (desde / hasta).
   */
  getAdminAnalytics(desde?: string | null, hasta?: string | null): Observable<AnalyticsDashboardResponse> {
    let params = new HttpParams();
    if (desde && desde.trim().length > 0) {
      params = params.set('desde', desde.trim()).set('dateFrom', desde.trim());
    }
    if (hasta && hasta.trim().length > 0) {
      params = params.set('hasta', hasta.trim()).set('dateTo', hasta.trim());
    }
    return this.http
      .get<ApiResponse<AnalyticsDashboardResponse>>(`${this.baseUrl}/admin`, { params })
      .pipe(map((res) => res.data));
  }

  /**
   * Obtiene el listado detallado de sesiones con alerta de frustración o bloqueo para el modal.
   * Permite filtrar por fecha de corte (desde / hasta).
   */
  getFrustrationDetails(aulaId?: string | null, desde?: string | null, hasta?: string | null): Observable<import('@models').FrustrationDetailResponse[]> {
    let params = new HttpParams();
    if (aulaId && aulaId.trim().length > 0) {
      params = params.set('aulaId', aulaId);
    }
    if (desde && desde.trim().length > 0) {
      params = params.set('desde', desde.trim()).set('dateFrom', desde.trim());
    }
    if (hasta && hasta.trim().length > 0) {
      params = params.set('hasta', hasta.trim()).set('dateTo', hasta.trim());
    }
    return this.http
      .get<ApiResponse<import('@models').FrustrationDetailResponse[]>>(`${this.baseUrl}/professional/frustration-details`, { params })
      .pipe(map((res) => res.data));
  }

  /**
   * Obtiene las métricas analíticas documentales y del workflow de reportes para el Administrador.
   * Permite filtrar por fecha de corte (desde / hasta).
   */
  getAdminReportsAnalytics(desde?: string | null, hasta?: string | null): Observable<import('@models').AdminReportsAnalyticsResponse> {
    let params = new HttpParams();
    if (desde && desde.trim().length > 0) {
      params = params.set('desde', desde.trim()).set('dateFrom', desde.trim());
    }
    if (hasta && hasta.trim().length > 0) {
      params = params.set('hasta', hasta.trim()).set('dateTo', hasta.trim());
    }
    return this.http
      .get<ApiResponse<import('@models').AdminReportsAnalyticsResponse>>(`${this.baseUrl}/admin/reports`, { params })
      .pipe(map((res) => res.data));
  }
}
