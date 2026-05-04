import { Type } from '@angular/core';

// ── Resultado que cada player emite al completar ──────────────────────────────
export interface PlayerResult {
  successPercentage: number; // 0–100
  timeSpentSeconds: number;
  requiredSupport?: boolean;
  observations?: string;
}

// ── Content shapes por tipo de plantilla ─────────────────────────────────────

export interface SelectFigureItem {
  id: string;
  pictogramId: number;
  label: string;
}
export interface SelectFigureContent {
  instruction: string;
  correctItemId: string;
  items: SelectFigureItem[];
}

export interface OrderSequenceItem {
  id: string;
  label: string;
  pictogramId?: number;
  correctPosition: number; // 0-based
}
export interface OrderSequenceContent {
  instruction: string;
  items: OrderSequenceItem[];
}

export interface MatchPair {
  id: string;
  label: string;
  pictogramId: number;
}
export interface MatchImageWordContent {
  instruction: string;
  pairs: MatchPair[];
}

export interface VisualSumOption {
  id: string;
  value: number;
}
export interface VisualSumContent {
  instruction: string;
  operandA: number;
  operandB: number;
  pictogramId?: number; // pictograma ilustrativo opcional
  options: VisualSumOption[];
}

// options: un array de opciones por cada letra oculta
// options[i] = letras posibles para hiddenIndices[i], la primera ES la correcta
export interface CompleteLetterContent {
  instruction: string;
  word: string;                // palabra completa, ej: "CASA"
  hiddenIndices: number[];     // índices de letras a ocultar (0-based), ej: [1, 3]
  options: string[][];         // options[i] = [correcta, distractor1, distractor2, ...]
}

// ── Registry: templateTypeCode → componente Angular ──────────────────────────
// Importaciones lazy para evitar ciclos — se completan en player-registry.ts
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type PlayerRegistry = Record<string, Type<any>>;
