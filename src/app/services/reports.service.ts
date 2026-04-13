import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse, PagedResponse } from '@models';
import { ReportListItemResponse, ReportResponse } from '@models/responses/reports/report.response';
import { GetReportsRequest } from '@models/requests/reports/get-reports.request';
import { CreateReportRequest } from '@models/requests/reports/create-report.request';
import { environment } from '@env';
import { unwrapResponse } from '@shared/utils';

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

    if (request.page) {
      params = params.set('page', request.page.toString());
    }
    if (request.pageSize) {
      params = params.set('pageSize', request.pageSize.toString());
    }
    if (request.search) {
      params = params.set('search', request.search);
    }
    if (request.personId) {
      params = params.set('personId', request.personId);
    }
    if (request.professionalId) {
      params = params.set('professionalId', request.professionalId);
    }
    if (request.reportTypeId) {
      params = params.set('reportTypeId', request.reportTypeId);
    }
    if (request.isActive !== undefined && request.isActive !== null) {
      params = params.set('isActive', request.isActive.toString());
    }
    if (request.sortBy) {
      params = params.set('sortBy', request.sortBy);
    }
    if (request.sortDirection) {
      params = params.set('sortDirection', request.sortDirection);
    }

    return this.http
      .get<ApiResponse<PagedResponse<ReportListItemResponse>>>(this.baseUrl, { params })
      .pipe(unwrapResponse());
  }

  getById(id: number): Observable<ReportResponse> {
    return this.http
      .get<ApiResponse<ReportResponse>>(`${this.baseUrl}/${id}`)
      .pipe(unwrapResponse());
  }

  create(request: CreateReportRequest): Observable<ReportResponse> {
    return this.http
      .post<ApiResponse<ReportResponse>>(this.baseUrl, request)
      .pipe(unwrapResponse());
  }
}