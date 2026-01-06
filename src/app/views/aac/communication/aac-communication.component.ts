import { Component } from '@angular/core';
import { BigButtonComponent } from '../../../shared/components/big-button/big-button.component';

@Component({
  selector: 'app-aac-communication',
  standalone: true,
  imports: [BigButtonComponent],
  template: `
    <div class="aac-communication">
      <h1 class="page-title">Comunicacion</h1>

      <section class="quick-phrases">
        <h2 class="section-title">Frases rapidas</h2>
        <div class="phrases-grid">
          @for (phrase of phrases; track phrase.id) {
            <app-big-button
              [label]="phrase.text"
              [icon]="phrase.icon"
              [color]="phrase.color"
              (buttonClick)="speak(phrase.text)"
            />
          }
        </div>
      </section>

      <section class="feelings">
        <h2 class="section-title">Como me siento</h2>
        <div class="feelings-grid">
          @for (feeling of feelings; track feeling.id) {
            <button
              class="feeling-btn"
              [style.--feeling-color]="feeling.color"
              (click)="speak('Me siento ' + feeling.label)"
              [attr.aria-label]="'Me siento ' + feeling.label">
              <span class="feeling-emoji">{{ feeling.emoji }}</span>
              <span class="feeling-label">{{ feeling.label }}</span>
            </button>
          }
        </div>
      </section>
    </div>
  `,
  styles: [`
    .aac-communication {
      padding: 8px;
    }

    .page-title {
      font-size: 28px;
      font-weight: 700;
      color: var(--aac-text, #1a1a1a);
      margin: 0 0 24px;
      text-align: center;
    }

    .section-title {
      font-size: 22px;
      font-weight: 600;
      color: var(--aac-text, #1a1a1a);
      margin: 0 0 16px;
    }

    .quick-phrases {
      margin-bottom: 32px;
    }

    .phrases-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 16px;
    }

    .feelings {
      margin-bottom: 24px;
    }

    .feelings-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 12px;
    }

    .feeling-btn {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 8px;
      padding: 20px 12px;
      background: white;
      border: 3px solid var(--feeling-color);
      border-radius: 20px;
      cursor: pointer;
      transition: all 0.15s ease;
    }

    .feeling-btn:hover {
      transform: scale(1.05);
      background: var(--feeling-color);

      .feeling-label {
        color: white;
      }
    }

    .feeling-btn:focus {
      outline: 4px solid #FFD700;
      outline-offset: 4px;
    }

    .feeling-emoji {
      font-size: 48px;
      line-height: 1;
    }

    .feeling-label {
      font-size: 16px;
      font-weight: 600;
      color: var(--aac-text, #1a1a1a);
    }

    :host-context([data-profile="high-contrast"]) {
      .page-title,
      .section-title {
        color: #fff;
      }

      .feeling-btn {
        background: #000;
        .feeling-label {
          color: #fff;
        }
      }
    }

    :host-context([data-color-mode="dark"]) {
      .page-title,
      .section-title {
        color: #f5f5f5;
      }

      .feeling-btn {
        background: #2a2a3e;
        .feeling-label {
          color: #f5f5f5;
        }
      }
    }

    @media (min-width: 600px) {
      .phrases-grid {
        grid-template-columns: repeat(4, 1fr);
      }

      .feelings-grid {
        grid-template-columns: repeat(6, 1fr);
      }
    }
  `]
})
export class AacCommunicationComponent {
  phrases = [
    { id: 1, text: 'Si', icon: 'cilCheckAlt', color: '#4CAF50' },
    { id: 2, text: 'No', icon: 'cilX', color: '#F44336' },
    { id: 3, text: 'Ayuda', icon: 'cilBell', color: '#FF9800' },
    { id: 4, text: 'Bano', icon: 'cilDoor', color: '#2196F3' },
    { id: 5, text: 'Agua', icon: 'cilDrop', color: '#00BCD4' },
    { id: 6, text: 'Comida', icon: 'cilFastfood', color: '#8BC34A' },
    { id: 7, text: 'Descanso', icon: 'cilBed', color: '#9C27B0' },
    { id: 8, text: 'Mas', icon: 'cilPlus', color: '#607D8B' }
  ];

  feelings = [
    { id: 1, emoji: '😊', label: 'Feliz', color: '#4CAF50' },
    { id: 2, emoji: '😢', label: 'Triste', color: '#2196F3' },
    { id: 3, emoji: '😠', label: 'Enojado', color: '#F44336' },
    { id: 4, emoji: '😰', label: 'Nervioso', color: '#FF9800' },
    { id: 5, emoji: '😴', label: 'Cansado', color: '#9C27B0' },
    { id: 6, emoji: '🤒', label: 'Enfermo', color: '#795548' }
  ];

  speak(text: string): void {
    if ('speechSynthesis' in window) {
      const utterance = new SpeechSynthesisUtterance(text);
      utterance.lang = 'es-ES';
      utterance.rate = 0.9;
      speechSynthesis.speak(utterance);
    }
  }
}
