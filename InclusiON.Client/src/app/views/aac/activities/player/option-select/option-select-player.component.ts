import { Component, inject, signal } from '@angular/core';
import { OptionSelectContent, OptionSelectOption } from '../player.models';
import { PlayerBaseComponent } from '../player-base.component';
import { PlayerIntroComponent } from '../components/player-intro.component';
import { PlayerResultComponent } from '../components/player-result.component';
import { ArasaacService } from '@services/arasaac.service';
import { AccessibilityService } from '@services/accessibility.service';

type OptionState = 'none' | 'correct' | 'wrong' | 'reveal' | 'dimmed';

@Component({
  selector: 'app-option-select-player',
  standalone: true,
  imports: [PlayerIntroComponent, PlayerResultComponent],
  templateUrl: './option-select-player.component.html',
  styleUrl: './option-select-player.component.scss',
})
export class OptionSelectPlayerComponent extends PlayerBaseComponent {
  private readonly arasaacService = inject(ArasaacService);
  private readonly a11y = inject(AccessibilityService);

  selectedOptionId = signal<string | null>(null);
  wrongOptionId = signal<string | null>(null);
  dockedOptionId = signal<string | null>(null);

  get content(): OptionSelectContent {
    try {
      const parsed = JSON.parse(this.assignment.contentJson);
      return {
        instruction: parsed.instruction ?? '',
        question: parsed.question ?? '',
        questionPictogramId: parsed.questionPictogramId,
        options: parsed.options ?? [],
        correctOptionId: parsed.correctOptionId ?? ''
      };
    }
    catch { return { instruction: '', question: '', options: [], correctOptionId: '' }; }
  }

  get options(): OptionSelectOption[] { return this.content.options; }
  get dockedOption(): OptionSelectOption | undefined {
    const id = this.dockedOptionId();
    return id ? this.options.find(o => o.id === id) : undefined;
  }
  get hint(): string {
    const count = this.options?.length ?? 0;
    return count > 0 ? `Hay ${count} opciones para elegir.` : 'Esta actividad aún no tiene contenido configurado.';
  }

  get correctText(): string {
    const c = this.content;
    return c.options.find(o => o.id === c.correctOptionId)?.text ?? '';
  }

  get resultMessage(): string {
    return this.isCorrect()
      ? '¡Correcto!'
      : `La respuesta correcta era: "${this.correctText}".`;
  }

  override startActivity(): void {
    super.startActivity();
    setTimeout(() => {
      this.speakQuestion();
    }, 500);
  }

  speakQuestion(): void {
    this.a11y.speak(this.content.question || '¿Qué va aquí?');
  }

  private playSound(type: 'success' | 'wrong'): void {
    try {
      const AudioCtx = window.AudioContext || (window as any).webkitAudioContext;
      if (!AudioCtx) return;
      const ctx = new AudioCtx();
      
      if (type === 'success') {
        const now = ctx.currentTime;
        const freqs = [523.25, 659.25, 783.99, 1046.50]; // C5, E5, G5, C6
        freqs.forEach((freq, idx) => {
          const osc = ctx.createOscillator();
          const gain = ctx.createGain();
          osc.type = 'triangle';
          osc.frequency.value = freq;
          
          gain.gain.setValueAtTime(0, now + idx * 0.08);
          gain.gain.linearRampToValueAtTime(0.15, now + idx * 0.08 + 0.04);
          gain.gain.exponentialRampToValueAtTime(0.0001, now + idx * 0.08 + 0.35);
          
          osc.connect(gain);
          gain.connect(ctx.destination);
          osc.start(now + idx * 0.08);
          osc.stop(now + idx * 0.08 + 0.4);
        });
      } else {
        const now = ctx.currentTime;
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();
        osc.type = 'sawtooth';
        osc.frequency.setValueAtTime(130, now);
        osc.frequency.linearRampToValueAtTime(90, now + 0.18);
        
        gain.gain.setValueAtTime(0.15, now);
        gain.gain.exponentialRampToValueAtTime(0.0001, now + 0.22);
        
        osc.connect(gain);
        gain.connect(ctx.destination);
        osc.start(now);
        osc.stop(now + 0.25);
      }
    } catch { /* AudioContext blocked or unsupported */ }
  }

  selectOption(option: OptionSelectOption): void {
    if (this.selectedOptionId() !== null) return;
    const correct = option.id === this.content.correctOptionId;
    this.selectedOptionId.set(option.id);
    this.isCorrect.set(correct);
    
    if (correct) {
      this.playSound('success');
      this.dockedOptionId.set(option.id);
      this.a11y.speak('¡Excelente!');
      setTimeout(() => this.phase.set('result'), 1800);
    } else {
      this.playSound('wrong');
      this.wrongOptionId.set(option.id);
      setTimeout(() => {
        this.wrongOptionId.set(null);
        this.selectedOptionId.set(null);
        this.isCorrect.set(null);
      }, 700);
    }
  }

  optionState(option: OptionSelectOption): OptionState {
    const sel = this.selectedOptionId();
    if (!sel) return 'none';
    if (option.id === sel) return this.isCorrect() ? 'correct' : 'wrong';
    if (option.id === this.content.correctOptionId && !this.isCorrect()) return 'reveal';
    return 'dimmed';
  }

  pictogramUrl(id: number): string { return this.arasaacService.getPictogramUrl(id); }

  onFinish(): void {
    this.finishActivity({
      successPercentage: this.isCorrect() ? 100 : 0,
      timeSpentSeconds: this.elapsedSeconds,
    });
  }

  override retry(): void {
    this.selectedOptionId.set(null);
    this.wrongOptionId.set(null);
    this.dockedOptionId.set(null);
    super.retry();
  }
}
