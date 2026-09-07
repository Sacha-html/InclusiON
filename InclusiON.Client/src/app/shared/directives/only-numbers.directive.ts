import { Directive, HostListener, inject } from '@angular/core';
import { NgControl } from '@angular/forms';

@Directive({
  selector: '[appOnlyNumbers], [onlyNumbers]',
  standalone: true,
})
export class OnlyNumbersDirective {
  private readonly ngControl = inject(NgControl, { optional: true });

  private readonly navigationKeys = [
    'Backspace',
    'Delete',
    'Tab',
    'Escape',
    'Enter',
    'ArrowLeft',
    'ArrowRight',
    'ArrowUp',
    'ArrowDown',
    'Home',
    'End',
  ];

  @HostListener('keydown', ['$event'])
  onKeyDown(event: KeyboardEvent): void {
    // Permitir teclas de control y navegación
    if (this.navigationKeys.includes(event.key)) {
      return;
    }

    // Permitir combinaciones de atajos de teclado (Ctrl/Cmd + A, C, V, X, Z)
    if (
      (event.ctrlKey || event.metaKey) &&
      ['a', 'c', 'v', 'x', 'z'].includes(event.key.toLowerCase())
    ) {
      return;
    }

    // Bloquear cualquier tecla que no sea un número (0-9)
    if (!/^[0-9]$/.test(event.key)) {
      event.preventDefault();
    }
  }

  @HostListener('input', ['$event'])
  onInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input) return;

    const originalValue = input.value;
    const maxLength = input.maxLength > 0 ? input.maxLength : Infinity;
    let sanitizedValue = originalValue.replace(/\D/g, '');
    if (sanitizedValue.length > maxLength) {
      sanitizedValue = sanitizedValue.substring(0, maxLength);
    }

    if (originalValue !== sanitizedValue) {
      input.value = sanitizedValue;
      if (this.ngControl?.control) {
        this.ngControl.control.setValue(sanitizedValue, { emitEvent: true });
      }
    }
  }

  @HostListener('paste', ['$event'])
  onPaste(event: ClipboardEvent): void {
    event.preventDefault();
    const clipboardData = event.clipboardData?.getData('text') ?? '';
    const digitsOnly = clipboardData.replace(/\D/g, '');

    if (!digitsOnly) return;

    const input = event.target as HTMLInputElement;
    const start = input.selectionStart ?? input.value.length;
    const end = input.selectionEnd ?? input.value.length;
    const maxLength = input.maxLength > 0 ? input.maxLength : Infinity;

    const currentValue = input.value;
    let newValue = currentValue.substring(0, start) + digitsOnly + currentValue.substring(end);
    if (newValue.length > maxLength) {
      newValue = newValue.substring(0, maxLength);
    }

    input.value = newValue;
    const newCursorPos = Math.min(start + digitsOnly.length, maxLength);
    input.setSelectionRange(newCursorPos, newCursorPos);

    if (this.ngControl?.control) {
      this.ngControl.control.setValue(newValue, { emitEvent: true });
    }
  }
}
