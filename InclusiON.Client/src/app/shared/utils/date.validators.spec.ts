import { FormControl, FormGroup } from '@angular/forms';
import { fakeAsync, tick } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import {
  notEqualToControlValidator,
  uniqueDocumentValidator,
  uniqueEmailValidator,
  uniqueLicenseValidator,
} from './date.validators';

describe('Uniqueness & cross-field validators', () => {
  describe('uniqueDocumentValidator', () => {
    it('returns null without calling backend when value is empty', fakeAsync(() => {
      const checkFn = jasmine.createSpy('checkDocument').and.returnValue(of({ isAvailable: true }));
      const control = new FormControl('', { asyncValidators: [uniqueDocumentValidator(checkFn)] });

      tick(600);
      expect(checkFn).not.toHaveBeenCalled();
      expect(control.errors).toBeNull();
    }));

    it('skips backend check when value matches initial value (case/trim insensitive)', fakeAsync(() => {
      const checkFn = jasmine.createSpy('checkDocument').and.returnValue(of({ isAvailable: false }));
      const control = new FormControl(' 30123456 ', {
        asyncValidators: [uniqueDocumentValidator(checkFn, () => '30123456')],
      });

      tick(600);
      expect(checkFn).not.toHaveBeenCalled();
      expect(control.errors).toBeNull();
    }));

    it('sets documentExists error with message and isInactiveStudent flag when unavailable', fakeAsync(() => {
      const checkFn = jasmine.createSpy('checkDocument').and.returnValue(
        of({
          isAvailable: false,
          message: 'El alumno está inactivo. Reactívelo desde el listado.',
          isInactiveStudent: true,
        })
      );
      const control = new FormControl('30123456', {
        asyncValidators: [uniqueDocumentValidator(checkFn)],
      });

      tick(500);
      expect(checkFn).toHaveBeenCalledWith('30123456');
      expect(control.errors).toEqual({
        documentExists: true,
        message: 'El alumno está inactivo. Reactívelo desde el listado.',
        isInactiveStudent: true,
      });
    }));

    it('clears errors when backend reports available', fakeAsync(() => {
      const checkFn = jasmine.createSpy('checkDocument').and.returnValue(of({ isAvailable: true }));
      const control = new FormControl('30999999', {
        asyncValidators: [uniqueDocumentValidator(checkFn)],
      });

      tick(500);
      expect(control.errors).toBeNull();
    }));

    it('does not block form if HTTP validation fails', fakeAsync(() => {
      const checkFn = jasmine.createSpy('checkDocument').and.returnValue(
        throwError(() => new Error('Network error'))
      );
      const control = new FormControl('30999999', {
        asyncValidators: [uniqueDocumentValidator(checkFn)],
      });

      tick(500);
      expect(control.errors).toBeNull();
    }));
  });

  describe('uniqueEmailValidator', () => {
    it('skips backend check when email matches initial value regardless of case', fakeAsync(() => {
      const checkFn = jasmine.createSpy('checkEmail').and.returnValue(of({ isAvailable: false }));
      const control = new FormControl('ADMIN@Test.COM', {
        asyncValidators: [uniqueEmailValidator(checkFn, () => 'admin@test.com')],
      });

      tick(600);
      expect(checkFn).not.toHaveBeenCalled();
      expect(control.errors).toBeNull();
    }));

    it('sets emailExists when unavailable', fakeAsync(() => {
      const checkFn = jasmine.createSpy('checkEmail').and.returnValue(
        of({ isAvailable: false, message: 'Email ya registrado.' })
      );
      const control = new FormControl('dup@test.com', {
        asyncValidators: [uniqueEmailValidator(checkFn)],
      });

      tick(500);
      expect(checkFn).toHaveBeenCalledWith('dup@test.com');
      expect(control.errors).toEqual({
        emailExists: true,
        message: 'Email ya registrado.',
      });
    }));
  });

  describe('uniqueLicenseValidator', () => {
    it('sets licenseExists when unavailable', fakeAsync(() => {
      const checkFn = jasmine.createSpy('checkLicense').and.returnValue(
        of({ isAvailable: false, message: 'Matrícula ya registrada.' })
      );
      const control = new FormControl('MP-12345', {
        asyncValidators: [uniqueLicenseValidator(checkFn)],
      });

      tick(500);
      expect(checkFn).toHaveBeenCalledWith('MP-12345');
      expect(control.errors).toEqual({
        licenseExists: true,
        message: 'Matrícula ya registrada.',
      });
    }));
  });

  describe('notEqualToControlValidator', () => {
    it('returns sameDocument error when values match ignoring whitespace and case', () => {
      const form = new FormGroup({
        studentDoc: new FormControl('30123456'),
        tutorDoc: new FormControl(' 30123456 ', [notEqualToControlValidator('studentDoc', 'sameDocument')]),
      });

      form.get('tutorDoc')?.updateValueAndValidity();
      expect(form.get('tutorDoc')?.errors).toEqual({ sameDocument: true });
    });

    it('returns null when values differ', () => {
      const form = new FormGroup({
        studentDoc: new FormControl('30123456'),
        tutorDoc: new FormControl('25111222', [notEqualToControlValidator('studentDoc', 'sameDocument')]),
      });

      form.get('tutorDoc')?.updateValueAndValidity();
      expect(form.get('tutorDoc')?.errors).toBeNull();
    });
  });
});
