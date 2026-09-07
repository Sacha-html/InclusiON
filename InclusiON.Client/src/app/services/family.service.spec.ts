import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { environment } from '@env';
import { FamilyService } from './family.service';

describe('FamilyService', () => {
  let service: FamilyService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [FamilyService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(FamilyService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('dispatches the dedicated family reactivation endpoint', () => {
    service.reactivateFamily('family-id').subscribe();

    const request = http.expectOne(`${environment.apiUrl}/Family/family-id/reactivate`);
    expect(request.request.method).toBe('PUT');
    request.flush({ success: true, data: { id: 'family-id', isActive: true } });
  });
});
