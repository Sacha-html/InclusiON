import { AbstractControl, AsyncValidatorFn, ValidationErrors } from '@angular/forms';
import { Observable, of, timer } from 'rxjs';
import { map, switchMap, debounceTime, distinctUntilChanged, catchError } from 'rxjs/operators';

export function validDate(control: AbstractControl): ValidationErrors | null {
  if (!control.value) return null;
  const value = control.value.trim();
  const regex = /^\d{2}\/\d{2}\/\d{4}$/;
  if (!regex.test(value)) return { invalidDate: true };
  const [day, month, year] = value.split('/').map(Number);
  const date = new Date(year, month - 1, day);
  if (date.getFullYear() !== year || date.getMonth() !== month - 1 || date.getDate() !== day) {
    return { invalidDate: true };
  }
  return null;
}

export function notFutureDate(control: AbstractControl): ValidationErrors | null {
  if (!control.value) return null;
  const value = control.value.trim();
  const regex = /^\d{2}\/\d{2}\/\d{4}$/;
  if (!regex.test(value)) return null;
  const [day, month, year] = value.split('/').map(Number);
  const date = new Date(year, month - 1, day);
  if (date > new Date()) return { futureDate: true };
  return null;
}

export function minAge(minAge: number) {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) return null;
    const value = control.value.trim();
    const regex = /^\d{2}\/\d{2}\/\d{4}$/;
    if (!regex.test(value)) return null;
    const [day, month, year] = value.split('/').map(Number);
    const birthDate = new Date(year, month - 1, day);
    const today = new Date();
    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();
    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
      age--;
    }
    if (age < minAge) return { minAge: true };
    return null;
  };
}

export function uniqueEmailValidator(
  checkFn: (email: string) => Observable<{ isAvailable: boolean; message?: string }>
): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    if (!control.value || control.value.trim() === '') return of(null);
    return timer(800).pipe(
      switchMap(() => checkFn(control.value.trim())),
      map(result => result.isAvailable ? null : { emailExists: true, message: result.message }),
      catchError(() => of(null))
    );
  };
}

export function uniqueLicenseValidator(
  checkFn: (license: string) => Observable<{ isAvailable: boolean; message?: string }>
): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    if (!control.value || control.value.trim() === '') return of(null);
    return timer(800).pipe(
      switchMap(() => checkFn(control.value.trim())),
      map(result => result.isAvailable ? null : { licenseExists: true, message: result.message }),
      catchError(() => of(null))
    );
  };
}

export function toIsoDate(ddmmyyyy: string): string {
  const [day, month, year] = ddmmyyyy.split('/');
  return `${year}-${month}-${day}T00:00:00`;
}

export function toDisplayDate(iso: string | undefined | null): string {
  if (!iso) return '';
  const d = new Date(iso);
  if (isNaN(d.getTime())) return '';
  const day = String(d.getDate()).padStart(2, '0');
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const year = d.getFullYear();
  return `${day}/${month}/${year}`;
}
