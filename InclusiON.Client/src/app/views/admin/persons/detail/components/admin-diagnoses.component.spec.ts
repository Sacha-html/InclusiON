import { TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { DiagnosesService } from '@services/diagnoses.service';
import { ToastService } from '@services';
import { AdminDiagnosesComponent } from './admin-diagnoses.component';

describe('AdminDiagnosesComponent diagnosis reactivation', () => {
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

  it('reactivates the encrypted diagnosis and updates the local row', () => {
    const patchStatus = jasmine.createSpy('patchStatus').and.returnValue(of(void 0));
    TestBed.configureTestingModule({
      imports: [AdminDiagnosesComponent],
      providers: [
        provideNoopAnimations(),
        { provide: DiagnosesService, useValue: { patchStatus } },
        { provide: ToastService, useValue: { success: jasmine.createSpy('success'), error: jasmine.createSpy('error') } },
      ],
    });
    const component = TestBed.createComponent(AdminDiagnosesComponent).componentInstance;
    component.diagnoses = [diagnosis];

    component.onRowAction({ action: 'activate', item: diagnosis });

    expect(patchStatus).toHaveBeenCalledWith(diagnosis.encryptedId, true);
    expect(component.diagnoses).toEqual([{ ...diagnosis, isActive: true }]);
  });

  it('keeps a deactivated row so it can be reactivated without reloading', () => {
    const patchStatus = jasmine.createSpy('patchStatus').and.returnValue(of(void 0));
    TestBed.configureTestingModule({
      imports: [AdminDiagnosesComponent],
      providers: [
        provideNoopAnimations(),
        { provide: DiagnosesService, useValue: { patchStatus } },
        { provide: ToastService, useValue: { success: jasmine.createSpy('success'), error: jasmine.createSpy('error') } },
      ],
    });
    const component = TestBed.createComponent(AdminDiagnosesComponent).componentInstance;
    component.diagnoses = [{ ...diagnosis, isActive: true }];
    component.deactivatingDiag = component.diagnoses[0];

    component.confirmDeactivate();

    expect(component.diagnoses).toEqual([{ ...diagnosis, isActive: false }]);
  });
});
