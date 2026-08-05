import { Type } from '@angular/core';
import { SelectFigurePlayerComponent }    from './select-figure/select-figure-player.component';
import { OrderSequencePlayerComponent }   from './order-sequence/order-sequence-player.component';
import { MatchImageWordPlayerComponent }  from './match-image-word/match-image-word-player.component';
import { VisualSumPlayerComponent }       from './visual-sum/visual-sum-player.component';
import { CompleteLetterPlayerComponent }  from './complete-letter/complete-letter-player.component';
import { OptionSelectPlayerComponent }    from './option-select/option-select-player.component';
import { GlobalReadingPlayerComponent }   from './global-reading/global-reading-player.component';

// Claves = Code de ActivityTemplateType en DB
// SOUND_RECOGNITION: sin player implementado aún (requiere MediaRecorder / audio API)
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export const PLAYER_REGISTRY: Record<string, Type<any>> = {
  PICTOGRAM_SELECT: SelectFigurePlayerComponent,   // seleccionar pictograma correcto
  ORDER_SEQUENCE:   OrderSequencePlayerComponent,   // ordenar secuencia
  BUILD_WORD:       CompleteLetterPlayerComponent,  // armar/completar palabra
  CLASSIFY:         MatchImageWordPlayerComponent,  // clasificar / emparejar imagen-palabra
  NUMERATION:       VisualSumPlayerComponent,       // numeración / suma visual
  OPTION_SELECT:    OptionSelectPlayerComponent,    // opción múltiple con texto + pictograma opcional
  GLOBAL_READING:   GlobalReadingPlayerComponent,   // lectura global: leer palabra, elegir imagen
  SOUND_RECOGNITION: OptionSelectPlayerComponent,   // conciencia fonológica (usar opción múltiple)
};
