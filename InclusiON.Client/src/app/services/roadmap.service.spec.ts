import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { RoadmapService } from './roadmap.service';
import { environment } from '@env';

describe('RoadmapService', () => {
  let service: RoadmapService;
  let http: HttpTestingController;
  const personId = 'person-id';

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [RoadmapService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(RoadmapService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('emits null for a successful response when the person has no roadmap', () => {
    let roadmap: unknown = 'not-empty';
    service.getRoadmap(personId).subscribe(data => roadmap = data);

    const request = http.expectOne(`${environment.apiUrl}/Persons/${personId}/roadmap`);
    expect(request.request.method).toBe('GET');
    request.flush({ success: true, data: null });

    expect(roadmap).toBeNull();
  });

  it('emits the created roadmap after the empty state flow creates one', () => {
    const createdRoadmap = { id: 7, encryptedId: 'roadmap-token', areas: [] };
    let received: unknown;
    service.createRoadmap(personId).subscribe(data => received = data);

    const request = http.expectOne(`${environment.apiUrl}/Persons/${personId}/roadmap`);
    expect(request.request.method).toBe('POST');
    request.flush({ success: true, data: createdRoadmap });

    expect(received).toEqual(createdRoadmap);
  });

  it('propagates real API errors instead of treating them as an empty state', () => {
    let errorStatus: number | undefined;
    service.getRoadmap(personId).subscribe({ error: error => errorStatus = error.status });

    const request = http.expectOne(`${environment.apiUrl}/Persons/${personId}/roadmap`);
    request.flush({ success: false, data: null, message: 'Database unavailable' }, {
      status: 500,
      statusText: 'Server Error',
    });

    expect(errorStatus).toBe(500);
  });
});
