import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, forwardRef, inject, Input, OnInit, QueryList, ViewChildren } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { CatalogsService } from '@services';
import { AvatarColorItem } from '@models';

/**
 * Selector accesible de color de avatar.
 * Renderiza una grilla de circulos de color con role=radiogroup.
 * Implementa ControlValueAccessor para integrarse con formControlName.
 */
@Component({
  selector: 'app-avatar-color-picker',
  standalone: true,
  imports: [],
  templateUrl: './avatar-color-picker.component.html',
  styleUrl: './avatar-color-picker.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => AvatarColorPickerComponent),
      multi: true,
    },
  ],
})
export class AvatarColorPickerComponent implements ControlValueAccessor, OnInit {
  private readonly catalogsService = inject(CatalogsService);
  private readonly cdr = inject(ChangeDetectorRef);

  @Input() ariaLabel = 'Seleccionar color de avatar';

  @ViewChildren('swatch') swatches!: QueryList<ElementRef<HTMLButtonElement>>;

  colors: AvatarColorItem[] = [];
  value: string | null = null;
  disabled = false;
  focusedIndex = 0;

  private onChange: (value: string | null) => void = () => {};
  private onTouched: () => void = () => {};

  ngOnInit(): void {
    this.catalogsService.getAvatarColors().subscribe({
      next: (data) => {
        this.colors = data;
        this.cdr.markForCheck();
      },
    });
  }

  getTabindex(hex: string, index: number): number {
    const selectedIdx = this.colors.findIndex(c => c.hex === this.value);
    const rovingTarget = selectedIdx >= 0 ? selectedIdx : 0;
    return index === rovingTarget ? 0 : -1;
  }

  onKeydown(event: KeyboardEvent): void {
    if (this.disabled || this.colors.length === 0) return;

    const len = this.colors.length;
    let next = this.focusedIndex;

    switch (event.key) {
      case 'ArrowRight':
      case 'ArrowDown':
        next = (this.focusedIndex + 1) % len;
        break;
      case 'ArrowLeft':
      case 'ArrowUp':
        next = (this.focusedIndex - 1 + len) % len;
        break;
      case 'Home':
        next = 0;
        break;
      case 'End':
        next = len - 1;
        break;
      default:
        return;
    }

    event.preventDefault();
    this.focusedIndex = next;
    const color = this.colors[next];
    this.select(color.hex);
    this.swatches.get(next)?.nativeElement.focus();
  }

  select(hex: string): void {
    if (this.disabled) return;
    this.value = hex;
    this.onChange(hex);
    this.onTouched();
    this.cdr.markForCheck();
  }

  writeValue(value: string | null): void {
    this.value = value;
    this.cdr.markForCheck();
  }

  registerOnChange(fn: (value: string | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
    this.cdr.markForCheck();
  }
}
