import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, switchMap, catchError, of } from 'rxjs';
import { ArasaacService, ArasaacPictogram } from '@services/arasaac.service';
import { ButtonDirective, ColComponent, FormControlDirective, RowComponent, SpinnerComponent } from '@coreui/angular';
import { ContentEditorBaseComponent } from '../content-editor-base.component';
import { SelectFigureContent } from '../../../../../aac/activities/player/player.models';

@Component({
  selector: 'app-select-figure-editor',
  standalone: true,
  imports: [FormsModule, ButtonDirective, ColComponent, FormControlDirective, RowComponent, SpinnerComponent],
  templateUrl: './select-figure-editor.component.html',
  styleUrl: './select-figure-editor.component.scss',
})
export class SelectFigureEditorComponent extends ContentEditorBaseComponent implements OnInit {
  private readonly arasaacService = inject(ArasaacService);

  content: SelectFigureContent = { instruction: '', correctItemId: '', items: [] };
  arasaacSearch  = '';
  arasaacResults = signal<ArasaacPictogram[]>([]);
  isSearching    = signal(false);
  private search$ = new Subject<string>();

  ngOnInit(): void {
    try { this.content = JSON.parse(this.initialJson); } catch { /* keep default */ }
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

  onArasaacSearchChange(term: string): void { this.search$.next(term); }

  addPictogram(pic: ArasaacPictogram): void {
    const id = crypto.randomUUID();
    this.content.items.push({ id, pictogramId: pic.id, label: pic.keyword });
    if (this.content.items.length === 1) this.content.correctItemId = id;
    this.emit();
  }

  removeItem(itemId: string): void {
    this.content.items = this.content.items.filter(i => i.id !== itemId);
    if (this.content.correctItemId === itemId) this.content.correctItemId = this.content.items[0]?.id ?? '';
    this.emit();
  }

  setCorrect(itemId: string): void { this.content.correctItemId = itemId; this.emit(); }
  onInstructionChange(): void { this.emit(); }

  pictogramUrl(id: number): string { return this.arasaacService.getPictogramUrl(id); }

  protected emit(): void {
    const valid = !!this.content.instruction.trim()
      && this.content.items.length >= 2
      && !!this.content.correctItemId
      && this.content.items.some(i => i.id === this.content.correctItemId);
    this.contentChange.emit(JSON.stringify(this.content));
    this.validChange.emit(valid);
  }
}
