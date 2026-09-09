import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { ToastService } from '@services';
import { UserManagementService } from '@services/user-management.service';
import { UserManagementDetailComponent } from './detail.component';

describe('User management detail actions', () => {
  const userService = {
    getUserDetail: jasmine.createSpy().and.returnValue(of(null)),
    getUserActivity: jasmine.createSpy().and.returnValue(of([])),
    resetPassword: jasmine.createSpy().and.returnValue(of({ temporaryPassword: 'Temporary1!', userEmail: 'user@test.com' })),
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [UserManagementDetailComponent],
      providers: [
        { provide: UserManagementService, useValue: userService },
        { provide: ToastService, useValue: { success: jasmine.createSpy(), error: jasmine.createSpy() } },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => null } } } },
        { provide: Router, useValue: { navigate: jasmine.createSpy() } },
      ],
    });
  });

  it('renders only password reset among account actions regardless of status', () => {
    const fixture = TestBed.createComponent(UserManagementDetailComponent);
    const component = fixture.componentInstance;
    const user = { userId: 'user-id', fullName: 'Test User', email: 'user@test.com', role: 'Professional', isActive: true } as any;
    component.user = user;
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Resetear contraseña');
    expect(fixture.nativeElement.textContent).not.toContain('Desactivar cuenta');
    expect(fixture.nativeElement.textContent).not.toContain('Reactivar cuenta');

    component.user = { ...user, isActive: false } as any;
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Resetear contraseña');
    expect(fixture.nativeElement.textContent).not.toContain('Desactivar cuenta');
    expect(fixture.nativeElement.textContent).not.toContain('Reactivar cuenta');
  });
});
