import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { DiagnosesService } from './diagnoses.service';
import { environment } from '@env';

describe('DiagnosesService', () => {
  let service: DiagnosesService;
  let http: HttpTestingController;
  const encryptedId = 'ENC:-diagnosis-token';

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [DiagnosesService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(DiagnosesService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('uses the encrypted id to visualize a diagnosis', () => {
    service.getById(encryptedId).subscribe();

    const request = http.expectOne(`${environment.apiUrl}/diagnoses/${encryptedId}`);
    expect(request.request.method).toBe('GET');
    request.flush({ success: true, data: { encryptedId, primaryDiagnosis: 'TEA' } });
  });

  it('uses the encrypted id to edit a diagnosis', () => {
    const requestBody = { diagnosisDate: '2026-01-10', primaryDiagnosis: 'TEA actualizado' };
    service.update(encryptedId, requestBody).subscribe();

    const request = http.expectOne(`${environment.apiUrl}/diagnoses/${encryptedId}`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(requestBody);
    request.flush({ success: true, data: { encryptedId, ...requestBody } });
  });

  it('uses the encrypted id for deactivation and reactivation', () => {
    service.patchStatus(encryptedId, false).subscribe();
    const deactivate = http.expectOne(`${environment.apiUrl}/diagnoses/${encryptedId}`);
    expect(deactivate.request.method).toBe('PATCH');
    expect(deactivate.request.body).toEqual({ isActive: false });
    deactivate.flush({ success: true, data: null });

    service.patchStatus(encryptedId, true).subscribe();
    const reactivate = http.expectOne(`${environment.apiUrl}/diagnoses/${encryptedId}`);
    expect(reactivate.request.method).toBe('PATCH');
    expect(reactivate.request.body).toEqual({ isActive: true });
    reactivate.flush({ success: true, data: null });
  });

  it('preserves an invalid encrypted id in the request for backend validation', () => {
    const invalidId = 'not-a-valid-encrypted-id';
    service.getById(invalidId).subscribe({ error: () => undefined });

    const request = http.expectOne(`${environment.apiUrl}/diagnoses/${invalidId}`);
    expect(request.request.method).toBe('GET');
    request.flush({ success: false, data: null, message: 'Identificador inválido.' }, { status: 400, statusText: 'Bad Request' });
  });
});
