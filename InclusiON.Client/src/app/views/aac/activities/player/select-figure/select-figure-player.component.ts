import { Component, inject, signal } from '@angular/core';
import { SelectFigureContent, SelectFigureItem } from '../player.models';
import { PlayerBaseComponent } from '../player-base.component';
import { PlayerIntroComponent } from '../components/player-intro.component';
import { PlayerResultComponent } from '../components/player-result.component';
import { PictogramCardComponent } from '../components/pictogram-card.component';
import { AccessibilityService } from '@services/accessibility.service';

type ItemState = 'none' | 'correct' | 'wrong' | 'reveal' | 'dimmed';

@Component({
  selector: 'app-select-figure-player',
  standalone: true,
  imports: [PlayerIntroComponent, PlayerResultComponent, PictogramCardComponent],
  templateUrl: './select-figure-player.component.html',
  styleUrl: './select-figure-player.component.scss',
})
export class SelectFigurePlayerComponent extends PlayerBaseComponent {
  private readonly a11y = inject(AccessibilityService);

  selectedItemId = signal<string | null>(null);

  get content(): SelectFigureContent {
    try {
      const parsed = JSON.parse(this.assignment.contentJson);
      return { instruction: parsed.instruction ?? '', correctItemId: parsed.correctItemId ?? '', items: parsed.items ?? [] };
    }
    catch { return { instruction: '', correctItemId: '', items: [] }; }
  }

  get items(): SelectFigureItem[]  { return this.content.items; }
  get hint(): string {
    const count = this.items?.length ?? 0;
    return count > 0 ? `Hay ${count} opciones para elegir.` : 'Esta actividad aún no tiene contenido configurado.';
  }

  get correctLabel(): string {
    const c = this.content;
    return c.items.find(i => i.id === c.correctItemId)?.label ?? '';
  }

  get resultMessage(): string {
    return this.isCorrect()
      ? 'Elegiste la respuesta correcta.'
      : `La respuesta correcta era ${this.correctLabel}.`;
  }

  // ── Fase playing ─────────────────────────────────────────────────────────
  selectItem(item: SelectFigureItem): void {
    if (this.selectedItemId() !== null) return;
    if (item.label) {
      this.a11y.speak(item.label);
    }
    const correct = item.id === this.content.correctItemId;
    this.selectedItemId.set(item.id);
    this.isCorrect.set(correct);
    setTimeout(() => this.phase.set('result'), 900);
  }

  itemState(item: SelectFigureItem): ItemState {
    const sel = this.selectedItemId();
    if (!sel) return 'none';
    if (item.id === sel)                                               return this.isCorrect() ? 'correct' : 'wrong';
    if (item.id === this.content.correctItemId && !this.isCorrect())  return 'reveal';
    return 'dimmed';
  }

  itemBadge(item: SelectFigureItem): string | undefined {
    const sel = this.selectedItemId();
    if (!sel) return undefined;
    if (item.id === sel && this.isCorrect())                          return '✅';
    if (item.id === sel && !this.isCorrect())                         return '❌';
    if (item.id === this.content.correctItemId && !this.isCorrect())  return '⭐';
    return undefined;
  }

  // ── Fase result ──────────────────────────────────────────────────────────
  onFinish(): void {
    this.finishActivity({
      successPercentage: this.isCorrect() ? 100 : 0,
      timeSpentSeconds:  this.elapsedSeconds,
    });
  }

  override retry(): void {
    this.selectedItemId.set(null);
    super.retry();
  }
}
