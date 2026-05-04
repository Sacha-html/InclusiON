import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonDirective, FormControlDirective } from '@coreui/angular';
import { ContentEditorBaseComponent } from '../content-editor-base.component';
import { CompleteLetterContent } from '../../../../../aac/activities/player/player.models';

const ALPHABET = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';

interface SlotOptions { correctLetter: string; distractors: string[]; }

@Component({
  selector: 'app-complete-letter-editor',
  standalone: true,
  imports: [FormsModule, ButtonDirective, FormControlDirective],
  templateUrl: './complete-letter-editor.component.html',
  styleUrl: './complete-letter-editor.component.scss',
})
export class CompleteLetterEditorComponent extends ContentEditorBaseComponent implements OnInit {

  instruction    = '';
  word           = '';
  hiddenIndices  = signal<number[]>([]);
  /** Keyed by letter index: [correct, distractors[]] */
  slotOptions    = signal<Map<number, SlotOptions>>(new Map());

  get upperWord(): string { return this.word.toUpperCase(); }

  ngOnInit(): void {
    try {
      const c: CompleteLetterContent = JSON.parse(this.initialJson);
      this.instruction = c.instruction ?? '';
      this.word        = c.word ?? '';
      const hidden = c.hiddenIndices ?? [];
      this.hiddenIndices.set(hidden);
      const map = new Map<number, SlotOptions>();
      hidden.forEach((letterIdx, slotIdx) => {
        const correct = this.upperWord[letterIdx] ?? '';
        const existing = c.options?.[slotIdx] ?? [];
        const distractors = existing.slice(1); // skip correct at index 0
        map.set(letterIdx, { correctLetter: correct, distractors: distractors.length ? distractors : this.generateDistractors(correct) });
      });
      this.slotOptions.set(map);
    } catch { /* keep defaults */ }
    this.emit();
  }

  letterArray(): string[] {
    return this.upperWord.split('');
  }

  isHidden(idx: number): boolean {
    return this.hiddenIndices().includes(idx);
  }

  toggleHidden(idx: number): void {
    const current = this.hiddenIndices();
    const letter = this.upperWord[idx];
    if (current.includes(idx)) {
      this.hiddenIndices.set(current.filter(i => i !== idx));
      this.slotOptions.update(map => { map.delete(idx); return new Map(map); });
    } else {
      this.hiddenIndices.set([...current, idx].sort((a, b) => a - b));
      this.slotOptions.update(map => {
        map.set(idx, { correctLetter: letter, distractors: this.generateDistractors(letter) });
        return new Map(map);
      });
    }
    this.emit();
  }

  generateDistractors(correct: string): string[] {
    const pool = ALPHABET.split('').filter(l => l !== correct.toUpperCase());
    const shuffled = pool.sort(() => Math.random() - 0.5);
    return shuffled.slice(0, 3);
  }

  regenerateDistractors(letterIdx: number): void {
    const slot = this.slotOptions().get(letterIdx);
    if (!slot) return;
    this.slotOptions.update(map => {
      map.set(letterIdx, { ...slot, distractors: this.generateDistractors(slot.correctLetter) });
      return new Map(map);
    });
    this.emit();
  }

  onWordChange(): void {
    // Remove hidden indices that are now out of bounds
    const upper = this.upperWord;
    const valid = this.hiddenIndices().filter(i => i < upper.length);
    // Update correctLetter for remaining slots if word changed
    this.slotOptions.update(map => {
      const newMap = new Map<number, SlotOptions>();
      valid.forEach(i => {
        const existing = map.get(i);
        const newCorrect = upper[i];
        if (existing && existing.correctLetter === newCorrect) {
          newMap.set(i, existing);
        } else {
          newMap.set(i, { correctLetter: newCorrect, distractors: this.generateDistractors(newCorrect) });
        }
      });
      return newMap;
    });
    this.hiddenIndices.set(valid);
    this.emit();
  }

  onInstructionChange(): void { this.emit(); }

  getSlot(idx: number): SlotOptions | undefined { return this.slotOptions().get(idx); }

  protected emit(): void {
    const hidden = this.hiddenIndices();
    const map = this.slotOptions();
    // Build options array ordered by hiddenIndices order
    const options: string[][] = hidden.map(letterIdx => {
      const slot = map.get(letterIdx);
      if (!slot) return [];
      return [slot.correctLetter, ...slot.distractors];
    });
    const content: CompleteLetterContent = {
      instruction: this.instruction,
      word:        this.upperWord,
      hiddenIndices: hidden,
      options,
    };
    const valid = !!this.instruction.trim()
      && this.word.length >= 2
      && hidden.length >= 1
      && options.every(o => o.length >= 2);
    this.contentChange.emit(JSON.stringify(content));
    this.validChange.emit(valid);
  }
}
