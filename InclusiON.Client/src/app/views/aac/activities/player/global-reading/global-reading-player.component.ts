import { Component, signal } from '@angular/core';
import { GlobalReadingContent } from '../player.models';
import { PlayerBaseComponent } from '../player-base.component';
import { PlayerIntroComponent } from '../components/player-intro.component';
import { PlayerResultComponent } from '../components/player-result.component';
import { PictogramCardComponent } from '../components/pictogram-card.component';

type ItemState = 'none' | 'correct' | 'wrong' | 'reveal' | 'dimmed';

@Component({
  selector: 'app-global-reading-player',
  standalone: true,
  imports: [PlayerIntroComponent, PlayerResultComponent, PictogramCardComponent],
  templateUrl: './global-reading-player.component.html',
  styleUrl: './global-reading-player.component.scss',
})
export class GlobalReadingPlayerComponent extends PlayerBaseComponent {

  selectedItemId = signal<string | null>(null);

  get content(): GlobalReadingContent {
    try { return JSON.parse(this.assignment.contentJson) as GlobalReadingContent; }
    catch { return { instruction: '', word: '', items: [], correctItemId: '' }; }
  }

  get items() { return this.content.items; }
  get hint(): string { return `Hay ${this.items.length} imágenes para elegir.`; }

  get correctLabel(): string {
    const c = this.content;
    return c.items.find(i => i.id === c.correctItemId)?.label ?? '';
  }

  get resultMessage(): string {
    return this.isCorrect()
      ? '¡Muy bien! Reconociste la palabra correctamente.'
      : `La imagen correcta era "${this.correctLabel}".`;
  }

  selectItem(item: { id: string; pictogramId: number; label: string }): void {
    if (this.selectedItemId() !== null) return;
    const correct = item.id === this.content.correctItemId;
    this.selectedItemId.set(item.id);
    this.isCorrect.set(correct);
    setTimeout(() => this.phase.set('result'), 900);
  }

  itemState(item: { id: string }): ItemState {
    const sel = this.selectedItemId();
    if (!sel) return 'none';
    if (item.id === sel) return this.isCorrect() ? 'correct' : 'wrong';
    if (item.id === this.content.correctItemId && !this.isCorrect()) return 'reveal';
    return 'dimmed';
  }

  itemBadge(item: { id: string }): string | undefined {
    const sel = this.selectedItemId();
    if (!sel) return undefined;
    if (item.id === sel && this.isCorrect()) return '✅';
    if (item.id === sel && !this.isCorrect()) return '❌';
    if (item.id === this.content.correctItemId && !this.isCorrect()) return '⭐';
    return undefined;
  }

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
