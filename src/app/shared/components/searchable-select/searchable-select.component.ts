import {
  Component,
  Input,
  OnInit,
  OnDestroy,
  forwardRef,
  signal,
  inject,
  HostListener,
  ElementRef,
  ChangeDetectionStrategy,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormsModule } from '@angular/forms';
import {
  Observable,
  Subject,
  debounceTime,
  distinctUntilChanged,
  switchMap,
  of,
  takeUntil,
  catchError,
} from 'rxjs';
import { SpinnerComponent } from '@coreui/angular';

/**
 * SearchableSelectComponent
 *
 * Combobox accesible con búsqueda server-side y debounce.
 * Reemplaza el patrón pageSize:1000 + Array.filter() client-side.
 *
 * Uso:
 *   <app-searchable-select
 *     formControlName="personId"
 *     [searchFn]="searchPersons"
 *     [displayFn]="displayPerson"
 *     [valueFn]="personId"
 *     placeholder="Buscar por nombre..."
 *   />
 */
@Component({
  selector: 'app-searchable-select',
  standalone: true,
  imports: [FormsModule, SpinnerComponent],
  templateUrl: './searchable-select.component.html',
  styleUrl: './searchable-select.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SearchableSelectComponent),
      multi: true,
    },
  ],
})
export class SearchableSelectComponent implements ControlValueAccessor, OnInit, OnDestroy {

  /** Función que recibe el query y devuelve Observable de resultados */
  @Input({ required: true }) searchFn!: (query: string) => Observable<any[]>;

  /** Cómo mostrar cada ítem en la lista y en el campo seleccionado */
  @Input({ required: true }) displayFn!: (item: any) => string;

  /** Qué valor emitir al form (por defecto: el ítem completo) */
  @Input() valueFn: (item: any) => any = (item: any) => item;

  /** Segunda línea de descripción opcional por ítem */
  @Input() subDisplayFn?: (item: any) => string;

  @Input() placeholder = 'Buscar...';
  @Input() minLength = 2;
  @Input() debounceMs = 350;
  @Input() noResultsText = 'Sin resultados';
  @Input() hintText = 'Escriba para buscar';

  private readonly el = inject(ElementRef);
  private readonly destroy$ = new Subject<void>();
  private readonly searchInput$ = new Subject<string>();

  readonly query = signal('');
  readonly isOpen = signal(false);
  readonly isLoading = signal(false);
  readonly results = signal<any[]>([]);
  readonly selectedItem = signal<any | null>(null);
  readonly activeIndex = signal(-1);
  readonly hasSearched = signal(false);

  isDisabled = false;
  readonly uid = `ss-${Math.random().toString(36).slice(2, 8)}`;

  private onChange: (value: any) => void = () => {};
  private onTouched: () => void = () => {};

  ngOnInit(): void {
    this.searchInput$.pipe(
      debounceTime(this.debounceMs),
      distinctUntilChanged(),
      switchMap(q => {
        if (q.length < this.minLength) {
          this.results.set([]);
          this.isLoading.set(false);
          this.hasSearched.set(false);
          return of([]);
        }
        this.isLoading.set(true);
        this.hasSearched.set(false);
        return this.searchFn(q).pipe(
          catchError(() => of([]))
        );
      }),
      takeUntil(this.destroy$)
    ).subscribe(items => {
      this.results.set(items);
      this.isLoading.set(false);
      this.hasSearched.set(true);
      this.activeIndex.set(-1);
    });
  }

  onInput(value: string): void {
    this.query.set(value);
    this.isOpen.set(true);
    this.searchInput$.next(value);
  }

  openDropdown(): void {
    if (this.results().length > 0) this.isOpen.set(true);
  }

  selectItem(item: any): void {
    this.selectedItem.set(item);
    this.query.set('');
    this.isOpen.set(false);
    this.results.set([]);
    this.hasSearched.set(false);
    this.onChange(this.valueFn(item));
    this.onTouched();
  }

  clearSelection(): void {
    this.selectedItem.set(null);
    this.results.set([]);
    this.hasSearched.set(false);
    this.onChange(null);
    this.onTouched();
  }

  @HostListener('keydown', ['$event'])
  onKeyDown(event: KeyboardEvent): void {
    const items = this.results();

    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        if (!this.isOpen() && items.length) { this.isOpen.set(true); return; }
        this.activeIndex.set(Math.min(this.activeIndex() + 1, items.length - 1));
        break;
      case 'ArrowUp':
        event.preventDefault();
        this.activeIndex.set(Math.max(this.activeIndex() - 1, 0));
        break;
      case 'Enter':
        event.preventDefault();
        if (this.isOpen() && this.activeIndex() >= 0 && items[this.activeIndex()]) {
          this.selectItem(items[this.activeIndex()]);
        }
        break;
      case 'Escape':
        this.isOpen.set(false);
        this.activeIndex.set(-1);
        break;
    }
  }

  @HostListener('document:click', ['$event'])
  onClickOutside(event: MouseEvent): void {
    if (!this.el.nativeElement.contains(event.target as Node)) {
      this.isOpen.set(false);
    }
  }

  // ───── ControlValueAccessor ─────────────────────────────────────────────

  writeValue(value: any): void {
    if (value === null || value === undefined) {
      this.selectedItem.set(null);
    }
    // Si el form ya tenía un valor (ej: edición) el padre puede pre-cargar
    // pasando el ítem completo via selectedItem directamente.
  }

  registerOnChange(fn: any): void { this.onChange = fn; }
  registerOnTouched(fn: any): void { this.onTouched = fn; }
  setDisabledState(isDisabled: boolean): void { this.isDisabled = isDisabled; }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
