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

  it('refreshes skill areas after invalidation and emits the newly created record', () => {
    const skillAreas: number[][] = [];
    service.getSkillAreas().subscribe(data => skillAreas.push(data.map(area => area.id)));

    const initialRequest = http.expectOne(`${environment.apiUrl}/Catalogs/skill-areas`);
    initialRequest.flush({ success: true, data: [{ id: 1, name: 'Existing' }] });

    service.invalidateSkillAreas();
    const refreshedRequest = http.expectOne(`${environment.apiUrl}/Catalogs/skill-areas`);
    refreshedRequest.flush({ success: true, data: [{ id: 1, name: 'Existing' }, { id: 2, name: 'Created' }] });

    expect(skillAreas).toEqual([[1], [1, 2]]);
  });

  it('refreshes disability types after invalidation and emits the newly created record', () => {
    const disabilityTypes: number[][] = [];
    service.getDisabilityTypes().subscribe(data => disabilityTypes.push(data.map(type => type.id)));

    const initialRequest = http.expectOne(`${environment.apiUrl}/Catalogs/disability-types`);
    initialRequest.flush({ success: true, data: [{ id: 1, name: 'Existing' }] });

    service.invalidateDisabilityTypes();
    const refreshedRequest = http.expectOne(`${environment.apiUrl}/Catalogs/disability-types`);
    refreshedRequest.flush({ success: true, data: [{ id: 1, name: 'Existing' }, { id: 2, name: 'Created' }] });

    expect(disabilityTypes).toEqual([[1], [1, 2]]);
  });

  it('refreshes autonomy levels after invalidation and emits the newly created record', () => {
    const autonomyLevels: number[][] = [];
    service.getAutonomyLevels().subscribe(data => autonomyLevels.push(data.map(level => level.id)));

    const initialRequest = http.expectOne(`${environment.apiUrl}/Catalogs/autonomy-levels`);
    initialRequest.flush({ success: true, data: [{ id: 1, name: 'Existing' }] });

    service.invalidateAutonomyLevels();
    const refreshedRequest = http.expectOne(`${environment.apiUrl}/Catalogs/autonomy-levels`);
    refreshedRequest.flush({ success: true, data: [{ id: 1, name: 'Existing' }, { id: 2, name: 'Created' }] });

    expect(autonomyLevels).toEqual([[1], [1, 2]]);
  });

  it('refreshes activity template types after invalidation', () => {
    const templates: number[][] = [];
    service.getActivityTemplateTypes().subscribe(data => templates.push(data.map(template => template.id)));

    const initialRequest = http.expectOne(`${environment.apiUrl}/Catalogs/activity-template-types`);
    initialRequest.flush({ success: true, data: [{ id: 1, name: 'Existing' }] });

    service.invalidateActivityTemplateTypes();
    const refreshedRequest = http.expectOne(`${environment.apiUrl}/Catalogs/activity-template-types`);
    refreshedRequest.flush({ success: true, data: [{ id: 1, name: 'Existing' }, { id: 2, name: 'Created' }] });

    expect(templates).toEqual([[1], [1, 2]]);
  });

  it('refreshes activity categories after invalidation and emits the newly created record', () => {
    const categories: number[][] = [];
    service.getActivityCategories().subscribe(data => categories.push(data.map(category => category.id)));

    const initialRequest = http.expectOne(`${environment.apiUrl}/Catalogs/activity-categories`);
    initialRequest.flush({ success: true, data: [{ id: 1, name: 'Existing' }] });

    service.invalidateActivityCategories();
    const refreshedRequest = http.expectOne(`${environment.apiUrl}/Catalogs/activity-categories`);
    refreshedRequest.flush({ success: true, data: [{ id: 1, name: 'Existing' }, { id: 2, name: 'Created' }] });

    expect(categories).toEqual([[1], [1, 2]]);
  });

  it('refreshes report types after invalidation', () => {
    const reportTypes: number[][] = [];
    service.getReportTypes().subscribe(data => reportTypes.push(data.map(type => type.id)));

    const initialRequest = http.expectOne(`${environment.apiUrl}/Catalogs/report-types`);
    initialRequest.flush({ success: true, data: [{ id: 1, name: 'Existing', isActive: true }] });

    service.invalidateReportTypes();
    const refreshedRequest = http.expectOne(`${environment.apiUrl}/Catalogs/report-types`);
    refreshedRequest.flush({ success: true, data: [{ id: 1, name: 'Existing', isActive: true }, { id: 2, name: 'Created', isActive: true }] });

    expect(reportTypes).toEqual([[1], [1, 2]]);
  });
});
