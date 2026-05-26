import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, switchMap, catchError, of } from 'rxjs';
import { ArasaacService, ArasaacPictogram } from '@services/arasaac.service';
import { ColComponent, FormControlDirective, RowComponent, SpinnerComponent } from '@coreui/angular';
import { ContentEditorBaseComponent } from '../content-editor-base.component';
import { OptionSelectContent, OptionSelectOption } from '../../../../../aac/activities/player/player.models';

@Component({
  selector: 'app-option-select-editor',
  standalone: true,
  imports: [FormsModule, ColComponent, FormControlDirective, RowComponent, SpinnerComponent],
  templateUrl: './option-select-editor.component.html',
  styleUrl: './option-select-editor.component.scss',
})
export class OptionSelectEditorComponent extends ContentEditorBaseComponent implements OnInit {
  private readonly arasaacService = inject(ArasaacService);

  content: OptionSelectContent = { instruction: '', question: '', options: [], correctOptionId: '' };

  newOptionText   = '';
  arasaacSearch   = '';
  arasaacResults  = signal<ArasaacPictogram[]>([]);
  isSearching     = signal(false);

  /** Índice de opción que tiene abierto el picker de pictograma (null = ninguno). */
  pickingPicFor   = signal<string | null>(null);

  private readonly search$ = new Subject<string>();

  ngOnInit(): void {
    try {
      const parsed = JSON.parse(this.initialJson);
      if (parsed && typeof parsed === 'object') {
        this.content = { instruction: '', question: '', options: [], correctOptionId: '', ...parsed };
      }
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

  addOption(): void {
    const text = this.newOptionText.trim();
    if (!text) return;
    const id = crypto.randomUUID();
    const option: OptionSelectOption = { id, text };
    this.content.options.push(option);
    if (this.content.options.length === 1) this.content.correctOptionId = id;
    this.newOptionText = '';
    this.emit();
  }

  removeOption(id: string): void {
    this.content.options = this.content.options.filter(o => o.id !== id);
    if (this.content.correctOptionId === id) {
      this.content.correctOptionId = this.content.options[0]?.id ?? '';
    }
    this.emit();
  }

  setCorrect(id: string): void { this.content.correctOptionId = id; this.emit(); }

  openPictogramPicker(optionId: string): void {
    this.pickingPicFor.set(optionId);
    this.arasaacSearch = '';
    this.arasaacResults.set([]);
  }

  closePictogramPicker(): void { this.pickingPicFor.set(null); }

  assignPictogram(pic: ArasaacPictogram): void {
    const targetId = this.pickingPicFor();
    if (!targetId) return;
    const option = this.content.options.find(o => o.id === targetId);
    if (option) { option.pictogramId = pic.id; }
    this.pickingPicFor.set(null);
    this.arasaacResults.set([]);
    this.emit();
  }

  clearPictogram(optionId: string): void {
    const option = this.content.options.find(o => o.id === optionId);
    if (option) { delete option.pictogramId; }
    this.emit();
  }

  onArasaacSearchChange(term: string): void { this.search$.next(term); }
  onInstructionChange(): void { this.emit(); }
  onQuestionChange(): void    { this.emit(); }

  pictogramUrl(id: number): string { return this.arasaacService.getPictogramUrl(id); }

  protected emit(): void {
    const valid = !!this.content.instruction.trim()
      && !!this.content.question.trim()
      && this.content.options.length >= 2
      && !!this.content.correctOptionId
      && this.content.options.some(o => o.id === this.content.correctOptionId);
    this.contentChange.emit(JSON.stringify(this.content));
    this.validChange.emit(valid);
  }
}
