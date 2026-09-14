import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { AuthService, ToastService } from '@services';
import { DiagnosesService } from '@services/diagnoses.service';
import { ProfessionalDiagnosesComponent } from './professional-diagnoses.component';

describe('ProfessionalDiagnosesComponent inactive diagnoses', () => {
  let fixture: ComponentFixture<ProfessionalDiagnosesComponent>;

  const diagnosis = {
    encryptedId: 'ENC:diagnosis',
    diagnosisDate: '2026-01-10',
    primaryDiagnosis: 'TEA',
    professionalName: 'Profesional',
    professionalId: 'professional-id',
    createdByUserId: 'user-id',
    createdAt: '2026-01-10T00:00:00Z',
    isActive: false,
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [ProfessionalDiagnosesComponent],
      providers: [
        provideNoopAnimations(),
        {
          provide: AuthService,
          useValue: {
            hasPermission: () => true,
            getCurrentUser: () => ({ id: 'user-id' }),
          },
        },
        { provide: DiagnosesService, useValue: { getById: () => of({ ...diagnosis }) } },
        { provide: ToastService, useValue: { error: jasmine.createSpy('error'), success: jasmine.createSpy('success') } },
      ],
    });
    fixture = TestBed.createComponent(ProfessionalDiagnosesComponent);
    fixture.componentInstance.personId = 'person-id';
    fixture.componentInstance.diagnoses = [diagnosis];
    fixture.detectChanges();
  });

  it('offers view instead of edit for an inactive diagnosis', () => {
    const action = fixture.nativeElement.querySelector('.timeline-content button') as HTMLButtonElement;

    expect(action.textContent?.trim()).toBe('Ver');
    expect(action.getAttribute('aria-label')).toBe('Ver diagnóstico');
  });

  it('keeps the inactive diagnosis read-only even if edit is invoked programmatically', () => {
    fixture.componentInstance.openEdit(diagnosis);
    fixture.detectChanges();

    expect(fixture.componentInstance.editingIsCreator()).toBeFalse();
    expect(fixture.nativeElement.textContent).toContain('Ver diagnóstico');
    expect(fixture.nativeElement.textContent).not.toContain('Guardar cambios');
  });
});
