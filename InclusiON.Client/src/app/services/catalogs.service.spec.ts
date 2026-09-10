import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { CatalogsService } from './catalogs.service';
import { environment } from '@env';

describe('CatalogsService', () => {
  let service: CatalogsService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [CatalogsService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(CatalogsService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify({ ignoreCancelled: true }));

  it('cancels an in-flight specialties request so stale data cannot overwrite the refresh', () => {
    const specialties: number[][] = [];
    service.getSpecialties().subscribe(data => specialties.push(data.map(specialty => specialty.id)));

    const initialRequest = http.expectOne(`${environment.apiUrl}/Catalogs/specialties`);
    service.invalidateSpecialties();
    const refreshedRequest = http.expectOne(`${environment.apiUrl}/Catalogs/specialties`);

    refreshedRequest.flush({ success: true, data: [{ id: 2, name: 'Updated', isActive: true }] });

    expect(initialRequest.cancelled).toBeTrue();
    expect(specialties).toEqual([[2]]);
  });
});
