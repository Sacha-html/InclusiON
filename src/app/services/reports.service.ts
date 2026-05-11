import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse, PagedResponse } from '@models';
import { ReportListItemResponse, ReportResponse } from '@models/responses/reports/report.response';
import { GetReportsRequest } from '@models/requests/reports/get-reports.request';
import { CreateReportRequest } from '@models/requests/reports/create-report.request';
import { environment } from '@env';
import { unwrapResponse, handleApiError } from '@shared/utils';
import { UpdateReportRequest } from '@models/requests/reports/update-report.request';

@Injectable({
  providedIn: 'root',
})
export class ReportsService {
  private readonly http = inject(HttpClient);

  private get baseUrl(): string {
    return `${environment.apiUrl}/reports`;
  }

  getReports(request: GetReportsRequest): Observable<PagedResponse<ReportListItemResponse>> {
    let params = new HttpParams();

    if (request.page) params = params.set('page', request.page.toString());
    if (request.pageSize) params = params.set('pageSize', request.pageSize.toString());
    if (request.search) params = params.set('search', request.search);
    if (request.personId) params = params.set('personId', request.personId);
    if (request.professionalId) params = params.set('professionalId', request.professionalId);
    if (request.reportTypeId) params = params.set('reportTypeId', request.reportTypeId.toString());
    if (request.isActive !== undefined && request.isActive !== null) {
      params = params.set('isActive', request.isActive.toString());
    }
    if (request.status) params = params.set('status', request.status);
    if (request.dateFrom) params = params.set('dateFrom', request.dateFrom);
    if (request.dateTo) params = params.set('dateTo', request.dateTo);
    if (request.sortBy) params = params.set('sortBy', request.sortBy);
    if (request.sortDirection) params = params.set('sortDirection', request.sortDirection);

    return this.http
      .get<ApiResponse<PagedResponse<ReportListItemResponse>>>(this.baseUrl, { params })
      .pipe(unwrapResponse());
  }

  /** Reportes para el familiar autenticado (solo aprobados de su persona a cargo) */
  getFamilyReports(request: GetReportsRequest): Observable<PagedResponse<ReportListItemResponse>> {
    let params = new HttpParams();

    if (request.page) params = params.set('page', request.page.toString());
    if (request.pageSize) params = params.set('pageSize', request.pageSize.toString());
    if (request.reportTypeId) params = params.set('reportTypeId', request.reportTypeId.toString());
    if (request.dateFrom) params = params.set('dateFrom', request.dateFrom);
    if (request.dateTo) params = params.set('dateTo', request.dateTo);
    if (request.sortBy) params = params.set('sortBy', request.sortBy);
    if (request.sortDirection) params = params.set('sortDirection', request.sortDirection);

    return this.http
      .get<ApiResponse<PagedResponse<ReportListItemResponse>>>(`${this.baseUrl}/family`, { params })
      .pipe(unwrapResponse());
  }

  getById(id: string): Observable<ReportResponse> {
    return this.http
      .get<ApiResponse<ReportResponse>>(`${this.baseUrl}/${id}`)
      .pipe(unwrapResponse());
  }

  create(request: CreateReportRequest): Observable<ReportResponse> {
    return this.http
      .post<ApiResponse<ReportResponse>>(this.baseUrl, request)
      .pipe(unwrapResponse());
  }

  /** Profesional envía el borrador al admin para revisión */
  submitReport(id: string): Observable<ReportResponse> {
    return this.http
      .patch<ApiResponse<ReportResponse>>(`${this.baseUrl}/${id}/submit`, {})
      .pipe(unwrapResponse());
  }

  /** Admin aprueba el reporte */
  approveReport(id: string): Observable<ReportResponse> {
    return this.http
      .patch<ApiResponse<ReportResponse>>(`${this.baseUrl}/${id}/approve`, {})
      .pipe(unwrapResponse());
  }

  update(id: string, request: UpdateReportRequest): Observable<ReportResponse> {
    return this.http
      .put<ApiResponse<ReportResponse>>(`${this.baseUrl}/${id}`, request)
      .pipe(unwrapResponse());
  }

  deactivate(id: string): Observable<void> {
    return this.http
      .put<void>(`${this.baseUrl}/${id}/deactivate`, {})
      .pipe(handleApiError());
  }

  /** Admin rechaza el reporte con comentario */
  rejectReport(id: string, comment: string): Observable<ReportResponse> {
    return this.http
      .patch<ApiResponse<ReportResponse>>(`${this.baseUrl}/${id}/reject`, { comment })
      .pipe(unwrapResponse());
  }
}
