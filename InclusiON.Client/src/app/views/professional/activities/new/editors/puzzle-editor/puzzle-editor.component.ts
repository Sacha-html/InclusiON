import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, switchMap, catchError, of } from 'rxjs';
import { ArasaacService, ArasaacPictogram } from '@services/arasaac.service';
import {
  ColComponent,
  FormControlDirective,
  FormCheckComponent,
  FormCheckInputDirective,
  FormCheckLabelDirective,
  FormSelectDirective,
  RowComponent,
  SpinnerComponent,
} from '@coreui/angular';
import { ContentEditorBaseComponent } from '../content-editor-base.component';
import { PuzzleContent } from '../../../../../aac/activities/player/player.models';

export interface PuzzleDifficultyOption {
  label: string;
  rows: number;
  cols: number;
  totalPieces: number;
}

export const PUZZLE_DIFFICULTY_OPTIONS: PuzzleDifficultyOption[] = [
  { label: '2 piezas (1 × 2 - Muy fácil)', rows: 1, cols: 2, totalPieces: 2 },
  { label: '4 piezas (2 × 2 - Fácil)',      rows: 2, cols: 2, totalPieces: 4 },
  { label: '6 piezas (2 × 3 - Medio)',      rows: 2, cols: 3, totalPieces: 6 },
  { label: '9 piezas (3 × 3 - Desafiante)', rows: 3, cols: 3, totalPieces: 9 },
];

@Component({
  selector: 'app-puzzle-editor',
  standalone: true,
  imports: [
    FormsModule,
    ColComponent,
    FormControlDirective,
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
    FormSelectDirective,
    RowComponent,
    SpinnerComponent,
  ],
  templateUrl: './puzzle-editor.component.html',
  styleUrl: './puzzle-editor.component.scss',
})
export class PuzzleEditorComponent extends ContentEditorBaseComponent implements OnInit {
  private readonly arasaacService = inject(ArasaacService);

  readonly difficultyOptions = PUZZLE_DIFFICULTY_OPTIONS;

  content: PuzzleContent = {
    instruction: 'Arma el rompecabezas',
    pictogramId: 0,
    label: '',
    rows: 2,
    cols: 2,
    showGhostGuide: true,
  };

  selectedDifficultyKey = '2x2';

  arasaacSearch  = '';
  arasaacResults = signal<ArasaacPictogram[]>([]);
  isSearching    = signal(false);
  private readonly search$ = new Subject<string>();

  ngOnInit(): void {
    try {
      const parsed = JSON.parse(this.initialJson);
      if (parsed && typeof parsed === 'object') {
        this.content = {
          instruction: parsed.instruction || 'Arma el rompecabezas',
          pictogramId: parsed.pictogramId || 0,
          label: parsed.label || '',
          rows: parsed.rows || 2,
          cols: parsed.cols || 2,
          showGhostGuide: parsed.showGhostGuide ?? true,
        };
        this.selectedDifficultyKey = `${this.content.rows}x${this.content.cols}`;
      }
    } catch {
      // keep default
    }

    this.search$.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      switchMap(term => {
        if (!term.trim()) return of([]);
        this.isSearching.set(true);
        return this.arasaacService.search(term).pipe(catchError(() => of([])));
      }),
    ).subscribe(r => {
      this.arasaacResults.set(r);
      this.isSearching.set(false);
    });

    this.emit();
  }

  onArasaacSearchChange(term: string): void {
    this.search$.next(term);
  }

  selectPictogram(pic: ArasaacPictogram): void {
    this.content.pictogramId = pic.id;
    this.content.label = pic.keyword;
    if (!this.content.instruction || this.content.instruction === 'Arma el rompecabezas') {
      this.content.instruction = `Arma el rompecabezas: ${pic.keyword}`;
    }
    this.emit();
  }

  clearPictogram(): void {
    this.content.pictogramId = 0;
    this.content.label = '';
    this.emit();
  }

  onDifficultyChange(value: string): void {
    const option = this.difficultyOptions.find(o => `${o.rows}x${o.cols}` === value);
    if (option) {
      this.content.rows = option.rows;
      this.content.cols = option.cols;
      this.emit();
    }
  }

  onGuideChange(): void {
    this.emit();
  }

  onInstructionChange(): void {
    this.emit();
  }

  getPictogramUrl(id: number): string | null {
    return this.arasaacService.getPictogramUrl(id);
  }

  protected emit(): void {
    const valid = !!this.content.instruction.trim()
      && this.content.pictogramId > 0
      && this.content.rows >= 1
      && this.content.cols >= 1;

    this.contentChange.emit(JSON.stringify(this.content));
    this.validChange.emit(valid);
  }
}
