import { Component, EventEmitter, Input, OnDestroy, OnInit, Output, signal } from '@angular/core';
import { inject } from '@angular/core';
import { ActivitiesService } from '@services/activities.service';
import {
  ActivityAssignmentResponse,
  SelectFigureContent,
  SelectFigureItem,
} from '@models/responses/activity.response';
import { ArasaacService } from '@services/arasaac.service';

type Phase = 'intro' | 'playing' | 'result';

@Component({
  selector: 'app-select-figure-player',
  standalone: true,
  imports: [],
  templateUrl: './select-figure-player.component.html',
  styleUrl: './select-figure-player.component.scss',
})
export class SelectFigurePlayerComponent implements OnInit, OnDestroy {
  @Input({ required: true }) assignment!: ActivityAssignmentResponse;
  @Output() completed = new EventEmitter<void>();

  private readonly activitiesService = inject(ActivitiesService);
  readonly arasaac                   = inject(ArasaacService);

  phase          = signal<Phase>('intro');
  isLoading      = signal(false);
  responseId     = signal<number | null>(null);
  selectedItemId = signal<string | null>(null);
  isCorrect      = signal<boolean | null>(null);

  private startTime = 0;

  get content(): SelectFigureContent {
    try { return JSON.parse(this.assignment.contentJson) as SelectFigureContent; }
    catch { return { instruction: '', correctItemId: '', items: [] }; }
  }

  get items(): SelectFigureItem[] { return this.content.items; }

  get correctLabel(): string {
    const c = this.content;
    return c.items.find(i => i.id === c.correctItemId)?.label ?? '';
  }

  ngOnInit(): void {}
  ngOnDestroy(): void {}

  // ── Fase 1: intro → playing ──────────────────────────────────────────────
  startActivity(): void {
    this.isLoading.set(true);
    this.activitiesService.startResponse(this.assignment.id).subscribe({
      next: (updatedAssignment) => {
        // El responseId es el último response en la lista (más reciente)
        const responses = updatedAssignment.responses ?? [];
        const latest    = responses.sort((a, b) =>
          new Date(b.startedAt).getTime() - new Date(a.startedAt).getTime()
        )[0];
        this.responseId.set(latest?.id ?? null);
        this.startTime = Date.now();
        this.isLoading.set(false);
        this.phase.set('playing');
      },
      error: () => this.isLoading.set(false),
    });
  }

  // ── Fase 2: playing → result ─────────────────────────────────────────────
  selectItem(item: SelectFigureItem): void {
    if (this.selectedItemId() !== null) return; // ya seleccionó
    const correct = item.id === this.content.correctItemId;
    this.selectedItemId.set(item.id);
    this.isCorrect.set(correct);

    // Transición automática a la pantalla de resultado
    setTimeout(() => this.phase.set('result'), 900);
  }

  // ── Fase 3: result → complete ────────────────────────────────────────────
  finishActivity(): void {
    const responseId = this.responseId();
    if (responseId === null) { this.completed.emit(); return; }

    this.isLoading.set(true);
    const timeSpent = Math.round((Date.now() - this.startTime) / 1000);
    const success   = this.isCorrect() ? 100 : 0;

    this.activitiesService.completeResponse(this.assignment.id, responseId, {
      successPercentage: success,
      timeSpentSeconds:  timeSpent,
      requiredSupport:   false,
    }).subscribe({
      next:  () => { this.isLoading.set(false); this.completed.emit(); },
      error: () => { this.isLoading.set(false); this.completed.emit(); },
    });
  }

  // ── Reintentar ───────────────────────────────────────────────────────────
  retry(): void {
    this.selectedItemId.set(null);
    this.isCorrect.set(null);
    this.phase.set('intro');
  }

  itemClass(item: SelectFigureItem): string {
    const sel = this.selectedItemId();
    if (!sel) return '';
    if (item.id === sel && this.isCorrect())            return 'item--correct';
    if (item.id === sel && !this.isCorrect())           return 'item--wrong';
    if (item.id === this.content.correctItemId && sel)  return 'item--reveal';
    return 'item--dimmed';
  }

  pictogramUrl(id: number): string {
    return this.arasaac.getPictogramUrl(id);
  }
}
