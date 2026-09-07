import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { AuthService, FamilyService, ToastService } from '@services';
import { ListComponent } from './list.component';

describe('Family list reactivation', () => {
  const familyService = {
    getFamily: jasmine.createSpy().and.returnValue(of({ data: [], totalRecords: 0 })),
    reactivateFamily: jasmine.createSpy().and.returnValue(of({ id: 'family-id', fullName: 'María López', isActive: true, temporaryPassword: 'Temporary1!' })),
  };
  const toastService = { success: jasmine.createSpy(), error: jasmine.createSpy() };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [ListComponent],
      providers: [
        { provide: FamilyService, useValue: familyService },
        { provide: AuthService, useValue: { hasPermission: () => true } },
        { provide: ToastService, useValue: toastService },
        { provide: Router, useValue: { navigate: jasmine.createSpy() } },
      ],
    });
  });

  it('shows the reactivation action for an inactive family and dispatches the dedicated service', () => {
    const component = TestBed.createComponent(ListComponent).componentInstance;
    const action = component.cols.find((column) => column.key === 'actions')!.actions!
      .find((item) => item.action === 'reactivate')!;

    expect(action.visible!({ isActive: false })).toBeTrue();
    expect(action.visible!({ isActive: true })).toBeFalse();

    component.onRowAction({ action: 'reactivate', item: { id: 'family-id', isActive: false } });
    component.confirmReactivate();

    expect(familyService.reactivateFamily).toHaveBeenCalledWith('family-id');
    expect(component.showPasswordModal).toBeTrue();
    expect(component.temporaryPassword).toBe('Temporary1!');
  });
});
