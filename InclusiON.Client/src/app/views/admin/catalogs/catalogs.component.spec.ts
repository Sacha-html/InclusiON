import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { CatalogsComponent } from './catalogs.component';
import { CatalogsService, CatalogAdminService, ToastService } from '@services';

describe('CatalogsComponent template types form', () => {
  let fixture: ComponentFixture<CatalogsComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [CatalogsComponent],
      providers: [
        provideNoopAnimations(),
        { provide: ActivatedRoute, useValue: { paramMap: of({ get: () => 'template-types' }) } },
        { provide: CatalogsService, useValue: { getSkillAreas: () => of([]), getActivityTemplateTypes: () => of([]) } },
        { provide: CatalogAdminService, useValue: {} },
        { provide: ToastService, useValue: {} },
      ],
    });
    fixture = TestBed.createComponent(CatalogsComponent);
  });

  it('only exposes administrator-facing fields and excludes backend metadata', () => {
    const component = fixture.componentInstance as any;
    component.catalogType = 'template-types';

    expect(component.config.fields.map((field: { key: string }) => field.key)).toEqual([
      'name', 'code', 'skillAreaId', 'usesPictograms', 'hasAudio', 'isActive',
    ]);
    expect(component.config.columns.map((column: { key: string }) => column.key)).toEqual([
      'name', 'code', 'skillAreaName', 'usesPictograms', 'hasAudio', 'isActive',
    ]);
    expect(component.config.fields.some((field: { key: string }) => field.key === 'contentSchema')).toBeFalse();
    expect(component.config.fields.some((field: { key: string }) => field.key === 'componentName')).toBeFalse();
    expect(component.config.columns.some((column: { key: string }) => column.key === 'contentSchema')).toBeFalse();
    expect(component.config.columns.some((column: { key: string }) => column.key === 'componentName')).toBeFalse();
  });
});
