import { AbstractControl, AsyncValidatorFn, ValidationErrors } from '@angular/forms';
import { Observable, of, timer } from 'rxjs';
import { map, switchMap, catchError } from 'rxjs/operators';

export function parseDateInput(value: unknown): Date | null {
  if (!value) return null;
  if (value instanceof Date) return isNaN(value.getTime()) ? null : value;
  const str = String(value).trim();
  if (!str) return null;

  // Formato ISO / type="date": YYYY-MM-DD
  const isoMatch = /^(\d{4})-(\d{2})-(\d{2})/.exec(str);
  if (isoMatch) {
    const year = Number(isoMatch[1]);
    const month = Number(isoMatch[2]);
    const day = Number(isoMatch[3]);
    const d = new Date(year, month - 1, day);
    if (d.getFullYear() === year && d.getMonth() === month - 1 && d.getDate() === day) {
      return d;
    }
    return null;
  }

  // Formato tradicional: DD/MM/YYYY
  const ddmmyyyyMatch = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(str);
  if (ddmmyyyyMatch) {
    const day = Number(ddmmyyyyMatch[1]);
    const month = Number(ddmmyyyyMatch[2]);
    const year = Number(ddmmyyyyMatch[3]);
    const d = new Date(year, month - 1, day);
    if (d.getFullYear() === year && d.getMonth() === month - 1 && d.getDate() === day) {
      return d;
    }
    return null;
  }

  const d = new Date(str);
  return isNaN(d.getTime()) ? null : d;
}

export function validDate(control: AbstractControl): ValidationErrors | null {
  if (!control.value) return null;
  const date = parseDateInput(control.value);
  if (!date) return { invalidDate: true };
  return null;
}

export function notFutureDate(control: AbstractControl): ValidationErrors | null {
  if (!control.value) return null;
  const date = parseDateInput(control.value);
  if (!date) return null;
  const today = new Date();
  today.setHours(23, 59, 59, 999);
  if (date > today) return { futureDate: true };
  return null;
}

export function calculateAgeFromDate(birthDate: Date): number {
  const today = new Date();
  let age = today.getFullYear() - birthDate.getFullYear();
  const monthDiff = today.getMonth() - birthDate.getMonth();
  if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
    age--;
  }
  return age;
}

/**
 * Validador de rango de edad (por defecto entre 12 y 40 años).
 * Si la edad calculada es menor a minAge o mayor a maxAge, retorna { ageOutOfRange: true }.
 */
export function ageRangeValidator(minAge = 12, maxAge = 40) {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) return null;
    const birthDate = parseDateInput(control.value);
    if (!birthDate) return null; // validDate se encarga del formato inválido

    const age = calculateAgeFromDate(birthDate);
    if (age < minAge || age > maxAge) {
      return {
        ageOutOfRange: true,
        minAge,
        maxAge,
        calculatedAge: age
      };
    }
    return null;
  };
}

export function minAge(minRequiredAge: number) {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) return null;
    const birthDate = parseDateInput(control.value);
    if (!birthDate) return null;
    const age = calculateAgeFromDate(birthDate);
    if (age < minRequiredAge) return { minAge: true, requiredAge: minRequiredAge, calculatedAge: age };
    return null;
  };
}

export function uniqueEmailValidator(
  checkFn: (email: string) => Observable<{ isAvailable: boolean; message?: string }>
): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    if (!control.value || String(control.value).trim() === '') return of(null);
    return timer(800).pipe(
      switchMap(() => checkFn(String(control.value).trim())),
      map(result => result.isAvailable ? null : { emailExists: true, message: result.message }),
      catchError(() => of(null))
    );
  };
}

export function uniqueLicenseValidator(
  checkFn: (license: string) => Observable<{ isAvailable: boolean; message?: string }>
): AsyncValidatorFn {
  return (control: AbstractControl): Observable<ValidationErrors | null> => {
    if (!control.value || String(control.value).trim() === '') return of(null);
    return timer(800).pipe(
      switchMap(() => checkFn(String(control.value).trim())),
      map(result => result.isAvailable ? null : { licenseExists: true, message: result.message }),
      catchError(() => of(null))
    );
  };
}

export function toIsoDate(dateValue: string): string {
  if (!dateValue) return '';
  const str = String(dateValue).trim();
  if (/^\d{4}-\d{2}-\d{2}/.test(str)) {
    return str.length === 10 ? `${str}T00:00:00` : str;
  }
  if (/^\d{2}\/\d{2}\/\d{4}$/.test(str)) {
    const [day, month, year] = str.split('/');
    return `${year}-${month}-${day}T00:00:00`;
  }
  return str;
}

export function toDisplayDate(iso: string | undefined | null): string {
  if (!iso) return '';
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return '';
  const day = String(d.getDate()).padStart(2, '0');
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const year = d.getFullYear();
  return `${day}/${month}/${year}`;
}

export function toInputDate(isoOrDate: string | Date | undefined | null): string {
  if (!isoOrDate) return '';
  const d = typeof isoOrDate === 'string' ? new Date(isoOrDate) : isoOrDate;
  if (Number.isNaN(d.getTime())) return '';
  const year = d.getFullYear();
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}
