import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { ToastService } from '@services';
import { UserRoles } from '@shared/constants/roles';
import { UserManagementService } from '@services/user-management.service';
import { UserManagementDetailComponent } from './detail.component';

describe('User management detail family actions', () => {
  const userService = {
    deactivateUser: jasmine.createSpy().and.returnValue(of(void 0)),
    reactivateUser: jasmine.createSpy().and.returnValue(of({ temporaryPassword: 'Temporary1!', userEmail: 'family@test.com' })),
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [UserManagementDetailComponent],
      providers: [
        { provide: UserManagementService, useValue: userService },
        { provide: ToastService, useValue: { success: jasmine.createSpy(), error: jasmine.createSpy() } },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => null } } },
        { provide: Router, useValue: { navigate: jasmine.createSpy() } },
      ],
    });
  });

  it('keeps password reset available but hides generic lifecycle actions for active and inactive family representatives', () => {
    const fixture = TestBed.createComponent(UserManagementDetailComponent);
    const component = fixture.componentInstance;
    component.user = { userId: 'family-id', role: UserRoles.FamilyRepresentative, isActive: true } as any;
    fixture.detectChanges();

    component.resetPassword();
    component.confirmDeactivate();
    component.reactivateUser();

    expect(component.showResetPasswordModal).toBeTrue();
    expect(component.showDeactivateModal).toBeFalse();
    expect(userService.deactivateUser).not.toHaveBeenCalled();
    expect(userService.reactivateUser).not.toHaveBeenCalled();
    expect(Array.from(fixture.nativeElement.querySelectorAll('button')).map((button: HTMLButtonElement) => button.textContent?.trim()))
      .toContain('Resetear contraseña');
    expect(fixture.nativeElement.textContent).not.toContain('Desactivar cuenta');
    expect(fixture.nativeElement.textContent).not.toContain('Reactivar cuenta');

    component.user = { userId: 'family-id', role: UserRoles.FamilyRepresentative, isActive: false } as any;
    fixture.detectChanges();

    expect(Array.from(fixture.nativeElement.querySelectorAll('button')).map((button: HTMLButtonElement) => button.textContent?.trim()))
      .toContain('Resetear contraseña');
    expect(fixture.nativeElement.textContent).not.toContain('Desactivar cuenta');
    expect(fixture.nativeElement.textContent).not.toContain('Reactivar cuenta');
  });
});
