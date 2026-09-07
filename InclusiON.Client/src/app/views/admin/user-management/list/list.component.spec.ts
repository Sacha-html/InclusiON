import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { AuthService, ToastService } from '@services';
import { UserManagementService } from '@services/user-management.service';
import { UserManagementListComponent } from './list.component';

describe('User management list family actions', () => {
  const userService = {
    getUsers: jasmine.createSpy().and.returnValue(of({ data: [], totalRecords: 0 })),
    deactivateUser: jasmine.createSpy().and.returnValue(of(void 0)),
    reactivateUser: jasmine.createSpy().and.returnValue(of({ temporaryPassword: 'Temporary1!', userEmail: 'family@test.com' })),
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [UserManagementListComponent],
      providers: [
        { provide: UserManagementService, useValue: userService },
        { provide: AuthService, useValue: {} },
        { provide: ToastService, useValue: { success: jasmine.createSpy(), error: jasmine.createSpy() } },
        { provide: Router, useValue: { navigate: jasmine.createSpy() } },
      ],
    });
  });

  it('keeps password reset but excludes family deactivation and reactivation', () => {
    const component = TestBed.createComponent(UserManagementListComponent).componentInstance;
    const actions = component.cols.find((column) => column.key === 'actions')!.actions!;
    const family = { userId: 'family-id', role: 'FamilyRepresentative', isActive: true } as any;

    expect(actions.find((action) => action.action === 'reset-password')!.visible!(family)).toBeTrue();
    expect(actions.find((action) => action.action === 'reset-password')!.visible!({ ...family, isActive: false })).toBeTrue();
    expect(actions.find((action) => action.action === 'deactivate')!.visible!(family)).toBeFalse();
    expect(actions.find((action) => action.action === 'reactivate')!.visible!({ ...family, isActive: false })).toBeFalse();

    component.onRowAction({ action: 'deactivate', item: family });
    component.onRowAction({ action: 'reactivate', item: { ...family, isActive: false } });

    expect(component.showConfirmModal).toBeFalse();
    expect(userService.deactivateUser).not.toHaveBeenCalled();
    expect(userService.reactivateUser).not.toHaveBeenCalled();
  });
});
