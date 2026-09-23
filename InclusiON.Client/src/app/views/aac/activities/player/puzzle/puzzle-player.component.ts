import { Component, computed, inject, signal } from '@angular/core';
import { NgStyle } from '@angular/common';
import { AccessibilityService } from '@services/accessibility.service';
import { ArasaacService } from '@services/arasaac.service';
import { PlayerBaseComponent } from '../player-base.component';
import { PlayerIntroComponent } from '../components/player-intro.component';
import { PlayerResultComponent } from '../components/player-result.component';
import { PuzzleContent, PuzzlePiece } from '../player.models';

@Component({
  selector: 'app-puzzle-player',
  standalone: true,
  imports: [NgStyle, PlayerIntroComponent, PlayerResultComponent],
  templateUrl: './puzzle-player.component.html',
  styleUrl: './puzzle-player.component.scss',
})
export class PuzzlePlayerComponent extends PlayerBaseComponent {
  private readonly a11y = inject(AccessibilityService);
  private readonly arasaacService = inject(ArasaacService);

  pieces = signal<PuzzlePiece[]>([]);
  selectedPieceId = signal<string | null>(null);
  draggedPieceId = signal<string | null>(null);
  isBoardComplete = signal(false);

  get content(): PuzzleContent {
    try {
      const parsed = JSON.parse(this.assignment.contentJson);
      const rows = Number(parsed.rows);
      const cols = Number(parsed.cols);
      return {
        instruction: parsed.instruction || 'Arma el rompecabezas',
        pictogramId: Number(parsed.pictogramId) || 0,
        label: parsed.label || '',
        rows: Number.isFinite(rows) && rows >= 1 ? rows : 2,
        cols: Number.isFinite(cols) && cols >= 1 ? cols : 2,
        showGhostGuide: parsed.showGhostGuide ?? true,
      };
    } catch {
      return {
        instruction: 'Arma el rompecabezas',
        pictogramId: 0,
        rows: 2,
        cols: 2,
        showGhostGuide: true,
      };
    }
  }

  get totalPieces(): number {
    return this.content.rows * this.content.cols;
  }

  get imageUrl(): string | null {
    return this.arasaacService.getPictogramUrl(this.content.pictogramId);
  }

  get hint(): string {
    return `Rompecabezas de ${this.totalPieces} piezas (${this.content.rows} × ${this.content.cols}).`;
  }

  // ── Computeds de estado de piezas y casilleros ────────────────────────────

  readonly bankPieces = computed(() => {
    return this.pieces().filter(p => p.currentSlotIndex === null);
  });

  readonly boardSlots = computed(() => {
    const total = this.totalPieces;
    const list: (PuzzlePiece | null)[] = Array(total).fill(null);
    for (const piece of this.pieces()) {
      if (piece.currentSlotIndex !== null && piece.currentSlotIndex >= 0 && piece.currentSlotIndex < total) {
        list[piece.currentSlotIndex] = piece;
      }
    }
    return list;
  });

  readonly placedCount = computed(() => {
    return this.pieces().filter(p => p.currentSlotIndex !== null).length;
  });

  readonly allPlaced = computed(() => {
    return this.pieces().length > 0 && this.placedCount() === this.totalPieces;
  });

  readonly score = computed(() => {
    const slots = this.boardSlots();
    if (!slots.length || this.totalPieces === 0) return 0;
    const correct = slots.filter((p, idx) => p !== null && p.originalIndex === idx).length;
    return Math.round((correct / this.totalPieces) * 100);
  });

  readonly selectedPiece = computed(() => {
    const selId = this.selectedPieceId();
    if (!selId) return null;
    return this.pieces().find(p => p.id === selId) ?? null;
  });

  resultMessage(): string {
    const s = this.score();
    if (s === 100) {
      return `¡Excelente! Armaste el rompecabezas completo en ${this.elapsedSeconds} segundos.`;
    }
    if (s >= 60) {
      return `¡Muy bien! Lograste un ${s}% de aciertos en ${this.elapsedSeconds} segundos.`;
    }
    return `Tuviste un ${s}% de aciertos. ¡Intentá de nuevo para ordenar todas las piezas!`;
  }

  // ── Inicialización ────────────────────────────────────────────────────────

  override startActivity(): void {
    this.initPuzzlePieces();
    super.startActivity();
    if (this.content.instruction) {
      setTimeout(() => this.a11y.speak(this.content.instruction), 250);
    }
  }

  private initPuzzlePieces(): void {
    const { rows, cols } = this.content;
    const total = rows * cols;
    const newPieces: PuzzlePiece[] = [];

    for (let i = 0; i < total; i++) {
      const r = Math.floor(i / cols);
      const c = i % cols;
      newPieces.push({
        id: `piece-${i}-${crypto.randomUUID()}`,
        originalIndex: i,
        row: r,
        col: c,
        currentSlotIndex: null,
      });
    }

    // Mezclar aleatoriamente las piezas asegurando que no queden en orden perfecto al inicio
    this.pieces.set(this.shuffleArray(newPieces));
    this.selectedPieceId.set(null);
    this.isBoardComplete.set(false);
  }

  private shuffleArray(array: PuzzlePiece[]): PuzzlePiece[] {
    const arr = [...array];
    for (let i = arr.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [arr[i], arr[j]] = [arr[j], arr[i]];
    }
    return arr;
  }

  // ── Recorte visual de piezas (CSS background) ─────────────────────────────

  getPieceBackgroundStyles(piece: PuzzlePiece): Record<string, string> {
    const { rows, cols } = this.content;
    const url = this.imageUrl;
    if (!url) return {};

    const bgSize = `${cols * 100}% ${rows * 100}%`;
    const posX = cols > 1 ? `${(piece.col / (cols - 1)) * 100}%` : '50%';
    const posY = rows > 1 ? `${(piece.row / (rows - 1)) * 100}%` : '50%';

    return {
      'background-image': `url("${url}")`,
      'background-size': bgSize,
      'background-position': `${posX} ${posY}`,
      'background-repeat': 'no-repeat',
    };
  }

  // ── Interacción por Clic / Tap (Accesibilidad para motricidad) ─────────────

  onPieceBankClick(piece: PuzzlePiece): void {
    if (this.isBoardComplete()) return;

    if (this.selectedPieceId() === piece.id) {
      this.selectedPieceId.set(null);
    } else {
      this.selectedPieceId.set(piece.id);
    }
  }

  onSlotClick(slotIndex: number): void {
    if (this.isBoardComplete()) return;

    const selected = this.selectedPiece();
    const currentPieceInSlot = this.boardSlots()[slotIndex];

    if (selected) {
      const updated = this.pieces().map(p => {
        if (p.id === selected.id) {
          return { ...p, currentSlotIndex: slotIndex };
        }
        if (currentPieceInSlot && p.id === currentPieceInSlot.id) {
          return { ...p, currentSlotIndex: selected.currentSlotIndex };
        }
        return p;
      });

      this.pieces.set(updated);
      this.selectedPieceId.set(null);
    } else if (currentPieceInSlot) {

      this.returnPieceToBank(currentPieceInSlot);
    }
  }

  returnPieceToBank(piece: PuzzlePiece): void {
    if (this.isBoardComplete()) return;
    const updated = this.pieces().map(p =>
      p.id === piece.id ? { ...p, currentSlotIndex: null } : p
    );
    this.pieces.set(updated);
    if (this.selectedPieceId() === piece.id) {
      this.selectedPieceId.set(null);
    }
  }

  // ── Interacción Drag & Drop ──────────────────────────────────────────────

  onDragStart(event: DragEvent, piece: PuzzlePiece): void {
    if (this.isBoardComplete()) {
      event.preventDefault();
      return;
    }
    this.draggedPieceId.set(piece.id);
    event.dataTransfer?.setData('text/plain', piece.id);
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move';
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = 'move';
    }
  }

  onDropSlot(event: DragEvent, slotIndex: number): void {
    event.preventDefault();
    const pieceId = this.draggedPieceId() || event.dataTransfer?.getData('text/plain');
    if (!pieceId) return;

    const piece = this.pieces().find(p => p.id === pieceId);
    if (!piece) return;

    const currentPieceInSlot = this.boardSlots()[slotIndex];
    const updated = this.pieces().map(p => {
      if (p.id === piece.id) {
        return { ...p, currentSlotIndex: slotIndex };
      }
      if (currentPieceInSlot && p.id === currentPieceInSlot.id) {
        return { ...p, currentSlotIndex: piece.currentSlotIndex };
      }
      return p;
    });

    this.pieces.set(updated);
    this.draggedPieceId.set(null);
    this.selectedPieceId.set(null);
  }


  onDropBank(event: DragEvent): void {
    event.preventDefault();
    const pieceId = this.draggedPieceId() || event.dataTransfer?.getData('text/plain');
    if (!pieceId) return;

    const updated = this.pieces().map(p =>
      p.id === pieceId ? { ...p, currentSlotIndex: null } : p
    );
    this.pieces.set(updated);
    this.draggedPieceId.set(null);
  }

  onDragEnd(): void {
    this.draggedPieceId.set(null);
  }

  // ── Comprobación y Finalización del Rompecabezas ──────────────────────────

  /**
   * Finaliza y evalúa el rompecabezas al presionar el botón Finalizar,
   * transitando a la pantalla de resultados para registrar el intento.
   */
  confirmPuzzle(): void {
    const s = this.score();
    const passed = s >= 60;
    this.isCorrect.set(passed);
    if (passed) {
      this.isBoardComplete.set(true);
      this.a11y.speak('¡Excelente! ¡Armaste el rompecabezas!');
    }
    this.phase.set('result');
  }


  // ── Fase result ──────────────────────────────────────────────────────────

  onFinish(): void {
    this.finishActivity({
      successPercentage: this.score(),
      timeSpentSeconds: this.elapsedSeconds,
    });
  }

  override retry(): void {
    this.initPuzzlePieces();
    super.retry();
  }

  speakInstruction(): void {
    if (this.content.instruction) {
      this.a11y.speak(this.content.instruction);
    }
  }
}
