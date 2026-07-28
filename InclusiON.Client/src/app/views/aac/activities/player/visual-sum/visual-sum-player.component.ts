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
    try { return JSON.parse(this.assignment.contentJson) as VisualSumContent; }
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
    if (this.selectedOptionId() !== null) return;
    const correct = option.value === this.correctValue;
    this.selectedOptionId.set(option.id);
    this.isCorrect.set(correct);
    setTimeout(() => this.phase.set('result'), 800);
  }

  optionState(option: VisualSumOption): 'correct' | 'wrong' | 'reveal' | 'dimmed' | 'none' {
    const sel = this.selectedOptionId();
    if (!sel) return 'none';
    if (option.id === sel)                                       return this.isCorrect() ? 'correct' : 'wrong';
    if (option.value === this.correctValue && !this.isCorrect()) return 'reveal';
    return 'dimmed';
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
