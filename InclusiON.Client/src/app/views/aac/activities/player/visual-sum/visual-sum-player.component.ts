import { Component, signal } from '@angular/core';
import { inject } from '@angular/core';
import { VisualSumContent, VisualSumOption } from '../player.models';
import { PlayerBaseComponent } from '../player-base.component';
import { PlayerIntroComponent } from '../components/player-intro.component';
import { PlayerResultComponent } from '../components/player-result.component';
import { ArasaacService } from '@services/arasaac.service';

@Component({
  selector: 'app-visual-sum-player',
  standalone: true,
  imports: [PlayerIntroComponent, PlayerResultComponent],
  templateUrl: './visual-sum-player.component.html',
  styleUrl:    './visual-sum-player.component.scss',
})
export class VisualSumPlayerComponent extends PlayerBaseComponent {

  readonly arasaac = inject(ArasaacService);

  selectedOptionId = signal<string | null>(null);

  get content(): VisualSumContent {
    try {
      const parsed = JSON.parse(this.assignment.contentJson);
      return {
        instruction: parsed.instruction ?? '',
        operandA: parsed.operandA ?? 0,
        operandB: parsed.operandB ?? 0,
        pictogramId: parsed.pictogramId,
        options: parsed.options ?? []
      };
    }
    catch { return { instruction: '', operandA: 0, operandB: 0, options: [] }; }
  }

  get correctValue(): number {
    return this.content.operandA + this.content.operandB;
  }

  get correctOption(): VisualSumOption | undefined {
    return this.content.options.find(o => o.value === this.correctValue);
  }

  get hint(): string { return 'Calculá la suma y elegí el resultado correcto.'; }

  // ── Generar array de "bolitas" para visualizar cada operando ──────────────
  dotsA(): number[] { return Array.from({ length: this.content.operandA }, (_, i) => i); }
  dotsB(): number[] { return Array.from({ length: this.content.operandB }, (_, i) => i); }

  // ── Selección ────────────────────────────────────────────────────────────
  selectOption(option: VisualSumOption): void {
    this.selectedOptionId.set(option.id);
  }

  confirmSelection(): void {
    const sel = this.selectedOptionId();
    if (!sel) return;
    const opt = this.content.options.find(o => o.id === sel);
    const correct = opt ? opt.value === this.correctValue : false;
    this.isCorrect.set(correct);
    this.phase.set('result');
  }

  optionState(option: VisualSumOption): 'correct' | 'wrong' | 'reveal' | 'dimmed' | 'none' {
    return 'none';
  }


  get resultMessage(): string {
    return this.isCorrect()
      ? `¡Correcto! ${this.content.operandA} + ${this.content.operandB} = ${this.correctValue}`
      : `La respuesta correcta era ${this.correctValue}.`;
  }

  onFinish(): void {
    this.finishActivity({
      successPercentage: this.isCorrect() ? 100 : 0,
      timeSpentSeconds:  this.elapsedSeconds,
    });
  }

  override retry(): void {
    this.selectedOptionId.set(null);
    super.retry();
  }
}
