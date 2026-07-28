import { Component, computed, signal } from '@angular/core';
import { inject } from '@angular/core';
import { MatchImageWordContent, MatchPair } from '../player.models';
import { PlayerBaseComponent } from '../player-base.component';
import { PlayerIntroComponent } from '../components/player-intro.component';
import { PlayerResultComponent } from '../components/player-result.component';
import { ArasaacService } from '@services/arasaac.service';

interface MatchState {
  imageId: string;
  wordId: string;
}

@Component({
  selector: 'app-match-image-word-player',
  standalone: true,
  imports: [PlayerIntroComponent, PlayerResultComponent],
  templateUrl: './match-image-word-player.component.html',
  styleUrl:    './match-image-word-player.component.scss',
})
export class MatchImageWordPlayerComponent extends PlayerBaseComponent {

  readonly arasaac = inject(ArasaacService);

  // Columnas barajadas independientemente
  shuffledImages = signal<MatchPair[]>([]);
  shuffledWords  = signal<MatchPair[]>([]);

  // Selección en curso
  selectedImageId = signal<string | null>(null);
  selectedWordId  = signal<string | null>(null);

  // Pares confirmados: imageId → wordId
  matches = signal<MatchState[]>([]);

  get content(): MatchImageWordContent {
    try { return JSON.parse(this.assignment.contentJson) as MatchImageWordContent; }
    catch { return { instruction: '', pairs: [] }; }
  }

  get hint(): string {
    return `Uní cada imagen con su palabra. Hay ${this.content.pairs.length} pares.`;
  }

  // IDs ya emparejados
  readonly matchedImageIds = computed(() => new Set(this.matches().map(m => m.imageId)));
  readonly matchedWordIds  = computed(() => new Set(this.matches().map(m => m.wordId)));

  // Score: % de pares correctos (imageId === wordId en nuestro modelo: el id es igual en imagen y palabra)
  readonly score = computed(() => {
    const ms = this.matches();
    if (!ms.length) return 0;
    const correct = ms.filter(m => m.imageId === m.wordId).length;
    return Math.round((correct / this.content.pairs.length) * 100);
  });

  readonly allMatched = computed(() =>
    this.matches().length === this.content.pairs.length
  );

  override startActivity(): void {
    const pairs = this.content.pairs;
    this.shuffledImages.set([...pairs].sort(() => Math.random() - 0.5));
    this.shuffledWords.set([...pairs].sort(() => Math.random() - 0.5));
    this.matches.set([]);
    this.selectedImageId.set(null);
    this.selectedWordId.set(null);
    super.startActivity();
  }

  // ── Selección ────────────────────────────────────────────────────────────
  selectImage(id: string): void {
    if (this.matchedImageIds().has(id)) return;
    this.selectedImageId.set(this.selectedImageId() === id ? null : id);
    this.tryMatch();
  }

  selectWord(id: string): void {
    if (this.matchedWordIds().has(id)) return;
    this.selectedWordId.set(this.selectedWordId() === id ? null : id);
    this.tryMatch();
  }

  private tryMatch(): void {
    const imgId  = this.selectedImageId();
    const wordId = this.selectedWordId();
    if (!imgId || !wordId) return;

    this.matches.update(prev => [...prev, { imageId: imgId, wordId }]);
    this.selectedImageId.set(null);
    this.selectedWordId.set(null);
  }

  undoLastMatch(): void {
    this.matches.update(prev => prev.slice(0, -1));
  }

  // ── Confirmar ────────────────────────────────────────────────────────────
  confirmMatches(): void {
    this.isCorrect.set(this.score() === 100);
    this.phase.set('result');
  }

  onFinish(): void {
    this.finishActivity({
      successPercentage: this.score(),
      timeSpentSeconds:  this.elapsedSeconds,
    });
  }

  override retry(): void {
    const pairs = this.content.pairs;
    this.shuffledImages.set([...pairs].sort(() => Math.random() - 0.5));
    this.shuffledWords.set([...pairs].sort(() => Math.random() - 0.5));
    this.matches.set([]);
    this.selectedImageId.set(null);
    this.selectedWordId.set(null);
    super.retry();
  }

  resultMessage(): string {
    const s = this.score();
    if (s === 100) return '¡Uniste todos los pares correctamente!';
    if (s >= 50)   return `Acertaste ${s}% de los pares. ¡Seguí practicando!`;
    return `Acertaste ${s}% de los pares. Intentá de nuevo.`;
  }

  // ── Helpers de estado visual ──────────────────────────────────────────────
  imageState(id: string): 'matched-correct' | 'matched-wrong' | 'selected' | 'none' {
    const match = this.matches().find(m => m.imageId === id);
    if (match) return match.imageId === match.wordId ? 'matched-correct' : 'matched-wrong';
    if (this.selectedImageId() === id) return 'selected';
    return 'none';
  }

  wordState(id: string): 'matched-correct' | 'matched-wrong' | 'selected' | 'none' {
    const match = this.matches().find(m => m.wordId === id);
    if (match) return match.imageId === match.wordId ? 'matched-correct' : 'matched-wrong';
    if (this.selectedWordId() === id) return 'selected';
    return 'none';
  }

  matchedWordFor(imageId: string): string | undefined {
    const match = this.matches().find(m => m.imageId === imageId);
    if (!match) return undefined;
    return this.content.pairs.find(p => p.id === match.wordId)?.label;
  }
}
