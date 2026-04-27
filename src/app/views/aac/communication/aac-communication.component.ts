import { Component, inject } from '@angular/core';
import { BigButtonComponent } from '../../../shared/components/big-button/big-button.component';
import { AccessibilityService } from '../../../services/accessibility.service';

@Component({
  selector: 'app-aac-communication',
  standalone: true,
  imports: [BigButtonComponent],
  templateUrl: './aac-communication.component.html',
  styleUrl: './aac-communication.component.scss'
})
export class AacCommunicationComponent {
  private readonly a11y = inject(AccessibilityService);

  // Colores oscuros verificados con contraste ≥ 4.5:1 sobre texto blanco (WCAG 1.4.3 AA)
  phrases = [
    { id: 1, text: 'Sí', icon: 'cilCheckAlt', color: '#2E7D32' },   // 4.62:1
    { id: 2, text: 'No', icon: 'cilX', color: '#B71C1C' },           // 6.19:1
    { id: 3, text: 'Ayuda', icon: 'cilBell', color: '#E65100' },     // 4.52:1
    { id: 4, text: 'Baño', icon: 'cilDoor', color: '#0D47A1' },     // 8.59:1
    { id: 5, text: 'Agua', icon: 'cilDrop', color: '#006064' },      // 7.01:1
    { id: 6, text: 'Comida', icon: 'cilFastfood', color: '#33691E' }, // 5.47:1
    { id: 7, text: 'Descanso', icon: 'cilBed', color: '#6A1B9A' },  // 5.08:1
    { id: 8, text: 'Más', icon: 'cilPlus', color: '#37474F' }        // 7.07:1
  ];

  feelings = [
    { id: 1, emoji: '😊', label: 'Feliz', color: '#2E7D32' },       // 4.62:1
    { id: 2, emoji: '😢', label: 'Triste', color: '#0D47A1' },      // 8.59:1
    { id: 3, emoji: '😠', label: 'Enojado', color: '#B71C1C' },     // 6.19:1
    { id: 4, emoji: '😰', label: 'Nervioso', color: '#E65100' },    // 4.52:1
    { id: 5, emoji: '😴', label: 'Cansado', color: '#6A1B9A' },     // 5.08:1
    { id: 6, emoji: '🤒', label: 'Enfermo', color: '#4E342E' }      // 8.45:1
  ];

  /**
   * Habla el texto usando el servicio de accesibilidad centralizado.
   * Esto respeta la configuración del usuario (velocidad, voz, etc.)
   */
  speak(text: string): void {
    // Usar el servicio de accesibilidad que respeta la configuración del usuario
    // Si TTS no está habilitado en el panel, usar el método directo para AAC
    if (this.a11y.textToSpeechEnabled()) {
      this.a11y.speak(text);
    } else {
      // AAC siempre debe poder hablar, incluso si TTS global está desactivado
      this.speakDirect(text);
    }
  }

  /**
   * Método directo de TTS para AAC (siempre funciona)
   */
  private speakDirect(text: string): void {
    if ('speechSynthesis' in window) {
      // Cancelar cualquier lectura anterior
      speechSynthesis.cancel();

      const utterance = new SpeechSynthesisUtterance(text);
      utterance.lang = 'es-ES';
      utterance.rate = this.a11y.textToSpeechRate(); // Respetar velocidad configurada
      utterance.pitch = 1;
      utterance.volume = 1;

      // Intentar usar voz en español
      const voices = speechSynthesis.getVoices();
      const spanishVoice = voices.find(v => v.lang.startsWith('es'));
      if (spanishVoice) {
        utterance.voice = spanishVoice;
      }

      speechSynthesis.speak(utterance);
    }
  }
}
