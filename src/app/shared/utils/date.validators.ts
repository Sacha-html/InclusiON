import { AbstractControl, ValidationErrors } from '@angular/forms';

export function validDate(control: AbstractControl): ValidationErrors | null {
  if (!control.value) return null;
  const regex = /^\d{2}\/\d{2}\/\d{4}$/;
  if (!regex.test(control.value)) return { invalidDate: true };
  const [day, month, year] = control.value.split('/').map(Number);
  const date = new Date(year, month - 1, day);
  if (date.getFullYear() !== year || date.getMonth() !== month - 1 || date.getDate() !== day) {
    return { invalidDate: true };
  }
  return null;
}

export function notFutureDate(control: AbstractControl): ValidationErrors | null {
  if (!control.value) return null;
  const regex = /^\d{2}\/\d{2}\/\d{4}$/;
  if (!regex.test(control.value)) return null;
  const [day, month, year] = control.value.split('/').map(Number);
  const date = new Date(year, month - 1, day);
  if (date > new Date()) return { futureDate: true };
  return null;
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
