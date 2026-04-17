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
  template: `
    <div class="picker" role="radiogroup" [attr.aria-label]="ariaLabel" (keydown)="onKeydown($event)">
      @for (c of colors; track c.hex; let i = $index) {
        <button
          #swatch
          type="button"
          class="swatch"
          role="radio"
          [class.selected]="c.hex === value"
          [attr.aria-checked]="c.hex === value"
          [attr.aria-label]="'Color ' + (c.name || c.hex)"
          [attr.title]="c.name || c.hex"
          [attr.tabindex]="getTabindex(c.hex, i)"
          [style.background-color]="c.hex"
          [disabled]="disabled"
          (click)="select(c.hex)"
          (focus)="focusedIndex = i">
          @if (c.hex === value) {
            <span class="check" aria-hidden="true">&#10003;</span>
          }
        </button>
      }
    </div>
  `,
  styles: [`
    .picker {
      display: flex;
      flex-wrap: wrap;
      gap: 10px;
      padding: 8px 0;
    }

    .swatch {
      width: 36px;
      height: 36px;
      min-width: 36px;
      border-radius: 50%;
      border: 2px solid var(--a11y-border, #E0E0E0);
      cursor: pointer;
      padding: 0;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-size: 18px;
      font-weight: 700;
      line-height: 1;
      transition: transform 0.15s ease, box-shadow 0.15s ease;
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.15);
    }

    @media (prefers-reduced-motion: reduce) {
      .swatch {
        transition: none;
      }
      .swatch:hover:not(:disabled) {
        transform: none;
      }
    }

    .swatch:hover:not(:disabled) {
      transform: scale(1.1);
    }

    .swatch:focus-visible {
      outline: 3px solid var(--a11y-focus-accent, #0D47A1);
      outline-offset: 3px;
    }

    .swatch.selected {
      border: 3px solid var(--a11y-text, #212121);
      box-shadow: 0 0 0 2px var(--a11y-bg, #ffffff), 0 2px 8px rgba(0, 0, 0, 0.25);
    }

    .swatch:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .check {
      text-shadow: 0 1px 2px rgba(0, 0, 0, 0.4);
    }
  `],
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
