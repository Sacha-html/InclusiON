import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, switchMap, catchError, of } from 'rxjs';
import { ArasaacService, ArasaacPictogram } from '@services/arasaac.service';
import { ButtonDirective, ColComponent, FormControlDirective, RowComponent, SpinnerComponent } from '@coreui/angular';
import { ContentEditorBaseComponent } from '../content-editor-base.component';
import { MatchImageWordContent } from '../../../../../aac/activities/player/player.models';

interface DraftPair { id: string; label: string; pictogramId?: number; }

@Component({
  selector: 'app-match-image-word-editor',
  standalone: true,
  imports: [FormsModule, ButtonDirective, ColComponent, FormControlDirective, RowComponent, SpinnerComponent],
  templateUrl: './match-image-word-editor.component.html',
  styleUrl: './match-image-word-editor.component.scss',
})
export class MatchImageWordEditorComponent extends ContentEditorBaseComponent implements OnInit {
  private readonly arasaacService = inject(ArasaacService);

  instruction    = '';
  pairs          = signal<DraftPair[]>([]);
  arasaacSearch  = '';
  arasaacResults = signal<ArasaacPictogram[]>([]);
  isSearching    = signal(false);
  activePairIdx  = signal(-1);
  private search$ = new Subject<string>();

  ngOnInit(): void {
    try {
      const c: MatchImageWordContent = JSON.parse(this.initialJson);
      this.instruction = c.instruction ?? '';
      this.pairs.set(c.pairs.map(p => ({ id: p.id, label: p.label, pictogramId: p.pictogramId })));
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

  addPair(): void {
    this.pairs.update(arr => [...arr, { id: crypto.randomUUID(), label: '' }]);
    this.emit();
  }

  removePair(idx: number): void {
    this.pairs.update(arr => arr.filter((_, i) => i !== idx));
    if (this.activePairIdx() === idx) this.activePairIdx.set(-1);
    this.emit();
  }

  selectPairForPicto(idx: number): void {
    this.activePairIdx.set(idx);
    this.arasaacSearch = '';
    this.arasaacResults.set([]);
  }

  onArasaacSearchChange(term: string): void { this.search$.next(term); }

  assignPictogram(pic: ArasaacPictogram): void {
    const idx = this.activePairIdx();
    if (idx < 0) return;
    this.pairs.update(arr => {
      const copy = [...arr];
      copy[idx] = { ...copy[idx], pictogramId: pic.id, label: copy[idx].label || pic.keyword };
      return copy;
    });
    this.activePairIdx.set(-1);
    this.arasaacResults.set([]);
    this.emit();
  }

  removePictogram(idx: number): void {
    this.pairs.update(arr => {
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
    const arr = this.pairs();
    const content: MatchImageWordContent = {
      instruction: this.instruction,
      pairs: arr.map(p => ({ id: p.id, label: p.label, pictogramId: p.pictogramId ?? 0 })),
    };
    const valid = !!this.instruction.trim()
      && arr.length >= 2
      && arr.every(p => !!p.label.trim() && !!p.pictogramId);
    this.contentChange.emit(JSON.stringify(content));
    this.validChange.emit(valid);
  }
}
