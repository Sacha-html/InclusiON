import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, switchMap, catchError, of } from 'rxjs';
import { ArasaacService, ArasaacPictogram } from '@services/arasaac.service';
import { ButtonDirective, ColComponent, FormControlDirective, RowComponent, SpinnerComponent } from '@coreui/angular';
import { ContentEditorBaseComponent } from '../content-editor-base.component';
import { OrderSequenceContent } from '../../../../../aac/activities/player/player.models';

interface DraftItem { id: string; label: string; pictogramId?: number; }

@Component({
  selector: 'app-order-sequence-editor',
  standalone: true,
  imports: [FormsModule, ButtonDirective, ColComponent, FormControlDirective, RowComponent, SpinnerComponent],
  templateUrl: './order-sequence-editor.component.html',
  styleUrl: './order-sequence-editor.component.scss',
})
export class OrderSequenceEditorComponent extends ContentEditorBaseComponent implements OnInit {
  private readonly arasaacService = inject(ArasaacService);

  instruction   = '';
  items         = signal<DraftItem[]>([]);
  arasaacSearch  = '';
  arasaacResults = signal<ArasaacPictogram[]>([]);
  isSearching    = signal(false);
  /** Index of item currently waiting for a pictogram from ARASAAC (-1 = none) */
  activeItemIdx  = signal(-1);
  private search$ = new Subject<string>();

  ngOnInit(): void {
    try {
      const c: OrderSequenceContent = JSON.parse(this.initialJson);
      this.instruction = c.instruction ?? '';
      this.items.set(c.items.map(it => ({ id: it.id, label: it.label, pictogramId: it.pictogramId })));
    } catch { /* keep defaults */ }
    this.search$.pipe(
      debounceTime(400), distinctUntilChanged(),
      switchMap(term => {
        if (!term.trim()) return of([]);
        this.isSearching.set(true);
        return this.arasaacService.search(term).pipe(catchError(() => of([])));
      }),
    ).subscribe(r => { this.arasaacResults.set(r); this.isSearching.set(false); });
    this.emit();
  }

  addItem(): void {
    this.items.update(arr => [...arr, { id: crypto.randomUUID(), label: '' }]);
    this.emit();
  }

  removeItem(idx: number): void {
    this.items.update(arr => arr.filter((_, i) => i !== idx));
    if (this.activeItemIdx() === idx) this.activeItemIdx.set(-1);
    this.emit();
  }

  moveUp(idx: number): void {
    if (idx === 0) return;
    this.items.update(arr => {
      const copy = [...arr];
      [copy[idx - 1], copy[idx]] = [copy[idx], copy[idx - 1]];
      return copy;
    });
    this.emit();
  }

  moveDown(idx: number): void {
    this.items.update(arr => {
      if (idx >= arr.length - 1) return arr;
      const copy = [...arr];
      [copy[idx], copy[idx + 1]] = [copy[idx + 1], copy[idx]];
      return copy;
    });
    this.emit();
  }

  selectItemForPicto(idx: number): void {
    this.activeItemIdx.set(idx);
    this.arasaacResults.set([]);
    this.arasaacSearch = '';
  }

  onArasaacSearchChange(term: string): void { this.search$.next(term); }

  assignPictogram(pic: ArasaacPictogram): void {
    const idx = this.activeItemIdx();
    if (idx < 0) return;
    this.items.update(arr => {
      const copy = [...arr];
      copy[idx] = { ...copy[idx], pictogramId: pic.id, label: copy[idx].label || pic.keyword };
      return copy;
    });
    this.activeItemIdx.set(-1);
    this.arasaacResults.set([]);
    this.emit();
  }

  removePictogram(idx: number): void {
    this.items.update(arr => {
      const copy = [...arr];
      const { pictogramId: _, ...rest } = copy[idx];
      copy[idx] = rest;
      return copy;
    });
    this.emit();
  }

  onLabelChange(): void { this.emit(); }
  onInstructionChange(): void { this.emit(); }

  pictogramUrl(id: number): string { return this.arasaacService.getPictogramUrl(id); }

  protected emit(): void {
    const arr = this.items();
    const content: OrderSequenceContent = {
      instruction: this.instruction,
      items: arr.map((it, i) => ({ id: it.id, label: it.label, pictogramId: it.pictogramId, correctPosition: i })),
    };
    const valid = !!this.instruction.trim() && arr.length >= 2 && arr.every(it => !!it.label.trim());
    this.contentChange.emit(JSON.stringify(content));
    this.validChange.emit(valid);
  }
}
