import { Type } from '@angular/core';
import { SelectFigureEditorComponent }   from './select-figure-editor/select-figure-editor.component';
import { OrderSequenceEditorComponent }  from './order-sequence-editor/order-sequence-editor.component';
import { MatchImageWordEditorComponent } from './match-image-word-editor/match-image-word-editor.component';
import { VisualSumEditorComponent }      from './visual-sum-editor/visual-sum-editor.component';
import { CompleteLetterEditorComponent } from './complete-letter-editor/complete-letter-editor.component';

// Claves = Code de ActivityTemplateType en DB
// OPTION_SELECT, GLOBAL_READING, SOUND_RECOGNITION: sin editor implementado aún
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export const CONTENT_EDITOR_REGISTRY: Record<string, Type<any>> = {
  PICTOGRAM_SELECT: SelectFigureEditorComponent,   // seleccionar pictograma correcto
  ORDER_SEQUENCE:   OrderSequenceEditorComponent,   // ordenar secuencia
  BUILD_WORD:       CompleteLetterEditorComponent,  // armar/completar palabra
  CLASSIFY:         MatchImageWordEditorComponent,  // clasificar / emparejar imagen-palabra
  NUMERATION:       VisualSumEditorComponent,       // numeración / suma visual
};
