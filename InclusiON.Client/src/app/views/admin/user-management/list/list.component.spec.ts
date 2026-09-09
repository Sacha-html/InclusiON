import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { AuthService, ToastService } from '@services';
import { UserManagementService } from '@services/user-management.service';
import { UserManagementListComponent } from './list.component';

describe('User management list actions', () => {
  const userService = {
    getUsers: jasmine.createSpy().and.returnValue(of({ data: [], totalRecords: 0 })),
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

  it('offers only view and password reset for every user status and role', () => {
    const component = TestBed.createComponent(UserManagementListComponent).componentInstance;
    const actions = component.cols.find((column) => column.key === 'actions')!.actions!;

    expect(actions.map((action) => action.action)).toEqual(['view', 'reset-password']);
    expect(actions.every((action) => !action.visible)).toBeTrue();
  });
});
