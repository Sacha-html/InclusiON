import { Component, computed, signal } from '@angular/core';
import { CompleteLetterContent } from '../player.models';
import { PlayerBaseComponent } from '../player-base.component';
import { PlayerIntroComponent } from '../components/player-intro.component';
import { PlayerResultComponent } from '../components/player-result.component';

/** Respuesta del usuario para cada hueco: índice en el array de opciones, o null si sin responder. */
type SlotAnswer = string | null;

@Component({
  selector: 'app-complete-letter-player',
  standalone: true,
  imports: [PlayerIntroComponent, PlayerResultComponent],
  templateUrl: './complete-letter-player.component.html',
  styleUrl:    './complete-letter-player.component.scss',
})
export class CompleteLetterPlayerComponent extends PlayerBaseComponent {

  // Respuestas actuales del usuario por hueco (índice = posición en hiddenIndices)
  answers = signal<SlotAnswer[]>([]);

  // Hueco seleccionado actualmente (el usuario elige opción para este)
  activeSlot = signal<number | null>(null);

  // Opciones barajadas por hueco (barajamos los distractores, no la correcta — se mezcla todo)
  shuffledOptions = signal<string[][]>([]);

  get content(): CompleteLetterContent {
    try { return JSON.parse(this.assignment.contentJson) as CompleteLetterContent; }
    catch { return { instruction: '', word: '', hiddenIndices: [], options: [] }; }
  }

  get hint(): string {
    return `Completá las ${this.content.hiddenIndices.length} letras que faltan.`;
  }

  /** Letras correctas (primera opción de cada slot según convención del modelo). */
  private get correctLetters(): string[] {
    return this.content.options.map(opts => opts[0]);
  }

  /** Score: % de huecos respondidos correctamente. */
  readonly score = computed(() => {
    const answers = this.answers();
    const correct = this.correctLetters;
    if (!correct.length) return 0;
    const hits = answers.filter((a, i) => a?.toUpperCase() === correct[i]?.toUpperCase()).length;
    return Math.round((hits / correct.length) * 100);
  });

  readonly allAnswered = computed(() =>
    this.answers().every(a => a !== null)
  );

  override startActivity(): void {
    const c = this.content;
    // Inicializar respuestas vacías
    this.answers.set(c.hiddenIndices.map(() => null));
    this.activeSlot.set(0); // empezar en el primer hueco
    // Barajar opciones de cada slot
    this.shuffledOptions.set(
      c.options.map(opts => [...opts].sort(() => Math.random() - 0.5))
    );
    super.startActivity();
  }

  // ── Interacción ──────────────────────────────────────────────────────────

  /** Selecciona qué hueco está activo. */
  selectSlot(slotIdx: number): void {
    this.activeSlot.set(slotIdx);
  }

  /** El usuario elige una letra para el slot activo. */
  pickLetter(letter: string): void {
    const slot = this.activeSlot();
    if (slot === null) return;
    this.answers.update(prev => {
      const next = [...prev];
      next[slot] = letter;
      return next;
    });
    // Avanzar al siguiente hueco sin responder
    this.advanceSlot(slot);
  }

  /** Borra la respuesta del slot activo. */
  clearSlot(slotIdx: number): void {
    this.answers.update(prev => {
      const next = [...prev];
      next[slotIdx] = null;
      return next;
    });
    this.activeSlot.set(slotIdx);
  }

  private advanceSlot(current: number): void {
    const answers = this.answers();
    // Buscar el próximo hueco sin responder
    for (let i = current + 1; i < answers.length; i++) {
      if (answers[i] === null) { this.activeSlot.set(i); return; }
    }
    // Si no hay uno adelante, buscar desde el inicio
    for (let i = 0; i < current; i++) {
      if (answers[i] === null) { this.activeSlot.set(i); return; }
    }
    // Todos respondidos
    this.activeSlot.set(null);
  }

  // ── Resultado ────────────────────────────────────────────────────────────

  confirmAnswer(): void {
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
    const c = this.content;
    this.answers.set(c.hiddenIndices.map(() => null));
    this.activeSlot.set(0);
    this.shuffledOptions.set(
      c.options.map(opts => [...opts].sort(() => Math.random() - 0.5))
    );
    super.retry();
  }

  resultMessage(): string {
    const s = this.score();
    if (s === 100) return `¡Perfecto! Escribiste "${this.content.word}" correctamente.`;
    if (s >= 50)   return `Acertaste ${s}% de las letras. ¡Seguí practicando!`;
    return `Acertaste ${s}% de las letras. Intentá de nuevo.`;
  }

  // ── Helpers de presentación ──────────────────────────────────────────────

  /**
   * Construye los "tokens" de la palabra para renderizar:
   * cada elemento es { letter, slotIdx } si es hueco, o { letter, slotIdx: null } si es visible.
   */
  wordTokens(): Array<{ letter: string; slotIdx: number | null }> {
    const c       = this.content;
    const word    = c.word.toUpperCase();
    const hidden  = new Map(c.hiddenIndices.map((charIdx, slotIdx) => [charIdx, slotIdx]));
    const answers = this.answers();

    return [...word].map((letter, charIdx) => {
      const slotIdx = hidden.get(charIdx) ?? null;
      return {
        letter: slotIdx !== null ? (answers[slotIdx] ?? '') : letter,
        slotIdx,
      };
    });
  }

  slotState(slotIdx: number): 'active' | 'filled' | 'empty' {
    if (this.activeSlot() === slotIdx) return 'active';
    return this.answers()[slotIdx] ? 'filled' : 'empty';
  }

  optionsForActiveSlot(): string[] {
    const slot = this.activeSlot();
    if (slot === null) return [];
    return this.shuffledOptions()[slot] ?? [];
  }
}
