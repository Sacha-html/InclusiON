import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, switchMap, catchError, of } from 'rxjs';
import { ArasaacService, ArasaacPictogram } from '@services/arasaac.service';
import { ButtonDirective, ColComponent, FormControlDirective, RowComponent, SpinnerComponent } from '@coreui/angular';
import { ContentEditorBaseComponent } from '../content-editor-base.component';
import { VisualSumContent, VisualSumOption } from '../../../../../aac/activities/player/player.models';

@Component({
  selector: 'app-visual-sum-editor',
  standalone: true,
  imports: [FormsModule, ButtonDirective, ColComponent, FormControlDirective, RowComponent, SpinnerComponent],
  templateUrl: './visual-sum-editor.component.html',
  styleUrl: './visual-sum-editor.component.scss',
})
export class VisualSumEditorComponent extends ContentEditorBaseComponent implements OnInit {
  private readonly arasaacService = inject(ArasaacService);

  instruction    = '';
  operandA       = 2;
  operandB       = 3;
  pictogramId?: number;
  options        = signal<VisualSumOption[]>([]);
  showArasaac    = false;
  arasaacSearch  = '';
  arasaacResults = signal<ArasaacPictogram[]>([]);
  isSearching    = signal(false);
  private search$ = new Subject<string>();

  get correctAnswer(): number { return this.operandA + this.operandB; }

  ngOnInit(): void {
    try {
      const c: VisualSumContent = JSON.parse(this.initialJson);
      this.instruction = c.instruction ?? '';
      this.operandA    = c.operandA ?? 2;
      this.operandB    = c.operandB ?? 3;
      this.pictogramId = c.pictogramId;
      this.options.set(c.options ?? []);
    } catch { /* keep defaults */ }
    if (this.options().length === 0) this.autoGenerateOptions();
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

  autoGenerateOptions(): void {
    const correct = this.correctAnswer;
    const candidates = [correct, correct + 1, correct - 1, correct + 2, correct + 3]
      .filter((v, i, arr) => v >= 0 && arr.indexOf(v) === i)
      .slice(0, 4);
    this.options.set(candidates.map(v => ({ id: crypto.randomUUID(), value: v })));
    this.emit();
  }

  onOperandChange(): void { this.autoGenerateOptions(); }
  onInstructionChange(): void { this.emit(); }
  onOptionChange(): void { this.emit(); }

  pictogramUrl(id: number): string { return this.arasaacService.getPictogramUrl(id); }

  toggleArasaac(): void {
    this.showArasaac = !this.showArasaac;
    if (!this.showArasaac) { this.arasaacSearch = ''; this.arasaacResults.set([]); }
  }

  onArasaacSearchChange(term: string): void { this.search$.next(term); }

  assignPictogram(pic: ArasaacPictogram): void {
    this.pictogramId = pic.id;
    this.showArasaac = false;
    this.arasaacResults.set([]);
    this.emit();
  }

  removePictogram(): void { this.pictogramId = undefined; this.emit(); }

  protected emit(): void {
    const opts = this.options();
    const content: VisualSumContent = {
      instruction: this.instruction,
      operandA:    this.operandA,
      operandB:    this.operandB,
      pictogramId: this.pictogramId,
      options:     opts,
    };
    const correct = this.correctAnswer;
    const valid = !!this.instruction.trim()
      && this.operandA >= 0
      && this.operandB >= 0
      && opts.some(o => o.value === correct);
    this.contentChange.emit(JSON.stringify(content));
    this.validChange.emit(valid);
  }
}
