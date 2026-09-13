import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { CatalogAdminService } from './catalog-admin.service';
import { environment } from '@env';

describe('CatalogAdminService activity template types', () => {
  let service: CatalogAdminService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [CatalogAdminService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(CatalogAdminService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('derives technical fields for create without requiring them from the form', () => {
    const request = {
      skillAreaId: 3,
      name: 'Image match',
      code: 'IMAGE_MATCH',
      usesPictograms: false,
      hasAudio: false,
    };

    service.createActivityTemplateType(request).subscribe();
    const req = http.expectOne(`${environment.apiUrl}/admin/catalogs/activity-template-types`);

    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      ...request,
      contentSchema: '{}',
      componentName: 'IMAGE_MATCH',
      displayOrder: 0,
    });
  });

  it('preserves technical values on update and sends the backend property names', () => {
    const request = {
      skillAreaId: 4,
      name: 'Updated template',
      code: 'UPDATED_TEMPLATE',
      usesPictograms: true,
      hasAudio: true,
      isActive: false,
    };

    service.updateActivityTemplateType('12', request, {
      contentSchema: '{"items":[]}',
      componentName: 'ExistingTemplateComponent',
      displayOrder: 9,
    }).subscribe();
    const req = http.expectOne(`${environment.apiUrl}/admin/catalogs/activity-template-types/12`);

    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({
      ...request,
      contentSchema: '{"items":[]}',
      componentName: 'ExistingTemplateComponent',
      displayOrder: 9,
    });
  });

  it('derives a safe fallback component identifier when code is blank', () => {
    service.createActivityTemplateType({
      skillAreaId: 3,
      name: 'Template',
      code: '   ',
      usesPictograms: false,
      hasAudio: false,
    }).subscribe();

    const req = http.expectOne(`${environment.apiUrl}/admin/catalogs/activity-template-types`);
    expect(req.request.body.componentName).toBe('TemplateType');
  });
});
