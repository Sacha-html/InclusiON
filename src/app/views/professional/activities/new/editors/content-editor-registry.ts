import { Type } from '@angular/core';
import { SelectFigureEditorComponent }   from './select-figure-editor/select-figure-editor.component';
import { OrderSequenceEditorComponent }  from './order-sequence-editor/order-sequence-editor.component';
import { MatchImageWordEditorComponent } from './match-image-word-editor/match-image-word-editor.component';
import { VisualSumEditorComponent }      from './visual-sum-editor/visual-sum-editor.component';
import { CompleteLetterEditorComponent } from './complete-letter-editor/complete-letter-editor.component';
import { OptionSelectEditorComponent }   from './option-select-editor/option-select-editor.component';
import { GlobalReadingEditorComponent }  from './global-reading-editor/global-reading-editor.component';

// Claves = Code de ActivityTemplateType en DB
// SOUND_RECOGNITION: sin editor implementado aún (requiere MediaRecorder / audio API)
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export const CONTENT_EDITOR_REGISTRY: Record<string, Type<any>> = {
  PICTOGRAM_SELECT: SelectFigureEditorComponent,   // seleccionar pictograma correcto
  ORDER_SEQUENCE:   OrderSequenceEditorComponent,   // ordenar secuencia
  BUILD_WORD:       CompleteLetterEditorComponent,  // armar/completar palabra
  CLASSIFY:         MatchImageWordEditorComponent,  // clasificar / emparejar imagen-palabra
  NUMERATION:       VisualSumEditorComponent,       // numeración / suma visual
  OPTION_SELECT:    OptionSelectEditorComponent,    // opción múltiple con texto + pictograma opcional
  GLOBAL_READING:   GlobalReadingEditorComponent,   // lectura global: leer palabra, elegir imagen
};
