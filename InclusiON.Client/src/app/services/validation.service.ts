import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env';
import { Observable } from 'rxjs';

export interface FieldAvailabilityResponse {
  isAvailable: boolean;
  message?: string;
  isInactiveStudent?: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class ValidationService {
  private readonly http = inject(HttpClient);

  private get apiUrl(): string {
    return `${environment.apiUrl}/validation`;
  }

  checkDocument(documentNumber: string, excludeEntityId?: string | null): Observable<FieldAvailabilityResponse> {
    let params = new HttpParams().set('documentNumber', documentNumber.trim());
    if (excludeEntityId) {
      params = params.set('excludeEntityId', excludeEntityId);
    }
    return this.http.get<FieldAvailabilityResponse>(`${this.apiUrl}/document`, { params });
  }

  checkEmail(
    email: string,
    options?: {
      excludeUserId?: string | null;
      excludeEntityId?: string | null;
      checkInvitations?: boolean;
    },
  ): Observable<FieldAvailabilityResponse> {
    let params = new HttpParams().set('email', email.trim());
    if (options?.excludeUserId) {
      params = params.set('excludeUserId', options.excludeUserId);
    }
    if (options?.excludeEntityId) {
      params = params.set('excludeEntityId', options.excludeEntityId);
    }
    if (options?.checkInvitations) {
      params = params.set('checkInvitations', 'true');
    }
    return this.http.get<FieldAvailabilityResponse>(`${this.apiUrl}/email`, { params });
  }

  checkLicenseNumber(licenseNumber: string, excludeProfessionalId?: string | null): Observable<FieldAvailabilityResponse> {
    let params = new HttpParams().set('licenseNumber', licenseNumber.trim());
    if (excludeProfessionalId) {
      params = params.set('excludeProfessionalId', excludeProfessionalId);
    }
    return this.http.get<FieldAvailabilityResponse>(`${this.apiUrl}/license-number`, { params });
  }
}
