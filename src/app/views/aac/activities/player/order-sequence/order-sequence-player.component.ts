import { Component, computed, signal } from '@angular/core';
import { OrderSequenceContent, OrderSequenceItem } from '../player.models';
import { PlayerBaseComponent } from '../player-base.component';
import { PlayerIntroComponent } from '../components/player-intro.component';
import { PlayerResultComponent } from '../components/player-result.component';
import { ArasaacService } from '@services/arasaac.service';
import { inject } from '@angular/core';

@Component({
  selector: 'app-order-sequence-player',
  standalone: true,
  imports: [PlayerIntroComponent, PlayerResultComponent],
  templateUrl: './order-sequence-player.component.html',
  styleUrl:    './order-sequence-player.component.scss',
})
export class OrderSequencePlayerComponent extends PlayerBaseComponent {

  readonly arasaac = inject(ArasaacService);

  // Lista de ítems en el orden actual (el estudiante la modifica)
  orderedItems = signal<OrderSequenceItem[]>([]);

  // Índice del ítem seleccionado (para mover con teclado/botones)
  selectedIndex = signal<number | null>(null);

  get content(): OrderSequenceContent {
    try { return JSON.parse(this.assignment.contentJson) as OrderSequenceContent; }
    catch { return { instruction: '', items: [] }; }
  }

  get hint(): string {
    return `Ordená los ${this.content.items.length} pasos en la secuencia correcta.`;
  }

  // Score: porcentaje de ítems que quedaron en la posición correcta
  readonly score = computed(() => {
    const items = this.orderedItems();
    if (!items.length) return 0;
    const correct = items.filter((item, idx) => item.correctPosition === idx).length;
    return Math.round((correct / items.length) * 100);
  });

  override startActivity(): void {
    // Mezclar los ítems antes de mostrar
    const shuffled = [...this.content.items].sort(() => Math.random() - 0.5);
    this.orderedItems.set(shuffled);
    this.selectedIndex.set(null);
    super.startActivity();
  }

  // ── Mover ítem ───────────────────────────────────────────────────────────
  selectItem(index: number): void {
    if (this.selectedIndex() === index) {
      this.selectedIndex.set(null); // deseleccionar
    } else {
      this.selectedIndex.set(index);
    }
  }

  moveUp(index: number): void {
    if (index === 0) return;
    this.swap(index, index - 1);
    this.selectedIndex.set(index - 1);
    setTimeout(() => {
      const items = document.querySelectorAll('.sequence-item');
      (items[index - 1] as HTMLElement)?.focus();
    }, 50);
  }

  moveDown(index: number): void {
    const items = this.orderedItems();
    if (index === items.length - 1) return;
    this.swap(index, index + 1);
    this.selectedIndex.set(index + 1);
    setTimeout(() => {
      const items = document.querySelectorAll('.sequence-item');
      (items[index + 1] as HTMLElement)?.focus();
    }, 50);
  }

  private swap(i: number, j: number): void {
    const items = [...this.orderedItems()];
    [items[i], items[j]] = [items[j], items[i]];
    this.orderedItems.set(items);
  }

  // ── Confirmar orden ──────────────────────────────────────────────────────
  confirmOrder(): void {
    const s = this.score();
    this.isCorrect.set(s === 100);
    this.phase.set('result');
  }

  onFinish(): void {
    this.finishActivity({
      successPercentage: this.score(),
      timeSpentSeconds:  this.elapsedSeconds,
    });
  }

  override retry(): void {
    const shuffled = [...this.content.items].sort(() => Math.random() - 0.5);
    this.orderedItems.set(shuffled);
    this.selectedIndex.set(null);
    super.retry();
  }

  resultMessage(): string {
    const s = this.score();
    if (s === 100) return '¡Ordenaste todos los pasos correctamente!';
    if (s >= 50)   return `Tuviste ${s}% de aciertos. ¡Seguí practicando!`;
    return `Tuviste ${s}% de aciertos. Intentá de nuevo.`;
  }
}
