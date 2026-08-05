import { Component, inject, signal } from '@angular/core';
import { OptionSelectContent, OptionSelectOption } from '../player.models';
import { PlayerBaseComponent } from '../player-base.component';
import { PlayerIntroComponent } from '../components/player-intro.component';
import { PlayerResultComponent } from '../components/player-result.component';
import { ArasaacService } from '@services/arasaac.service';

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

  selectedOptionId = signal<string | null>(null);

  get content(): OptionSelectContent {
    try {
      const parsed = JSON.parse(this.assignment.contentJson);
      return { instruction: parsed.instruction ?? '', question: parsed.question ?? '', options: parsed.options ?? [], correctOptionId: parsed.correctOptionId ?? '' };
    }
    catch { return { instruction: '', question: '', options: [], correctOptionId: '' }; }
  }

  get options(): OptionSelectOption[] { return this.content.options; }
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

  selectOption(option: OptionSelectOption): void {
    if (this.selectedOptionId() !== null) return;
    const correct = option.id === this.content.correctOptionId;
    this.selectedOptionId.set(option.id);
    this.isCorrect.set(correct);
    setTimeout(() => this.phase.set('result'), 900);
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
    super.retry();
  }
}
