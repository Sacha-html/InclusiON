import { Component, inject, HostListener, ElementRef, ViewChild, AfterViewInit, OnDestroy, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IconModule } from '@coreui/icons-angular';
import { ButtonModule, FormModule, TooltipModule } from '@coreui/angular';
import {
  AccessibilityService,
  AccessibilityTheme,
  AccessibilityProfile,
  ColorMode
} from '../../services/accessibility.service';

@Component({
  selector: 'app-accessibility-panel',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    IconModule,
    ButtonModule,
    FormModule,
    TooltipModule
  ],
  templateUrl: './accessibility-panel.component.html',
  styleUrl: './accessibility-panel.component.scss'
})
export class AccessibilityPanelComponent implements AfterViewInit, OnDestroy {
  readonly a11y = inject(AccessibilityService);
  private readonly elementRef = inject(ElementRef);
  private readingGuideElement: HTMLElement | null = null;
  private mouseMoveHandler: ((e: MouseEvent) => void) | null = null;
  private focusHandler: ((e: FocusEvent) => void) | null = null;

  @ViewChild('panelContainer') panelContainer!: ElementRef<HTMLDivElement>;
  @ViewChild('closeButton') closeButton!: ElementRef<HTMLButtonElement>;

  constructor() {
    // Efecto para manejar la guía de lectura
    effect(() => {
      if (this.a11y.readingGuide()) {
        this.enableReadingGuide();
      } else {
        this.disableReadingGuide();
      }
    });
  }

  // Atajos de teclado globales
  @HostListener('document:keydown', ['$event'])
  handleKeyboardShortcut(event: KeyboardEvent): void {
    // Alt + A para toggle del panel de accesibilidad
    if (event.altKey && event.key.toLowerCase() === 'a') {
      event.preventDefault();
      this.a11y.togglePanel();
      if (this.a11y.panelOpen()) {
        setTimeout(() => this.focusFirstElement(), 100);
      }
    }

    // Alt + S para leer texto seleccionado (cuando TTS está habilitado)
    if (event.altKey && event.key.toLowerCase() === 's') {
      if (this.a11y.textToSpeechEnabled()) {
        event.preventDefault();
        this.a11y.speakSelection();
      }
    }

    // Alt + X para detener lectura
    if (event.altKey && event.key.toLowerCase() === 'x') {
      if (this.a11y.isSpeaking()) {
        event.preventDefault();
        this.a11y.stopSpeaking();
      }
    }

    // Escape: primero cierra el panel, luego sale del modo lectura
    if (event.key === 'Escape') {
      if (this.a11y.panelOpen()) {
        this.a11y.closePanel();
      } else if (this.a11y.readingMode()) {
        event.preventDefault();
        this.a11y.updateSetting('readingMode', false);
      }
    }
  }

  ngAfterViewInit(): void {
    // Inicializar guía de lectura si está activa
    if (this.a11y.readingGuide()) {
      this.enableReadingGuide();
    }
  }

  ngOnDestroy(): void {
    this.disableReadingGuide();
  }

  private enableReadingGuide(): void {
    if (this.readingGuideElement) return;

    // Crear elemento de guía de lectura
    this.readingGuideElement = document.createElement('div');
    this.readingGuideElement.className = 'a11y-reading-guide-bar';
    this.readingGuideElement.setAttribute('aria-hidden', 'true');
    document.body.appendChild(this.readingGuideElement);

    // Throttle para mejor rendimiento
    let ticking = false;
    let lastY = 0;

    // Handler para seguir el mouse con throttling
    this.mouseMoveHandler = (e: MouseEvent) => {
      lastY = e.clientY;
      if (!ticking) {
        requestAnimationFrame(() => {
          if (this.readingGuideElement) {
            this.readingGuideElement.style.top = `${lastY}px`;
          }
          ticking = false;
        });
        ticking = true;
      }
    };

    document.addEventListener('mousemove', this.mouseMoveHandler);

    // Soporte para teclado: seguir el elemento enfocado
    this.focusHandler = (e: FocusEvent) => {
      if (!this.readingGuideElement) return;
      const target = e.target as HTMLElement;
      if (target) {
        const rect = target.getBoundingClientRect();
        const centerY = rect.top + rect.height / 2;
        this.readingGuideElement.style.top = `${centerY}px`;
      }
    };
    document.addEventListener('focusin', this.focusHandler);
  }

  private disableReadingGuide(): void {
    if (this.readingGuideElement) {
      this.readingGuideElement.remove();
      this.readingGuideElement = null;
    }

    if (this.mouseMoveHandler) {
      document.removeEventListener('mousemove', this.mouseMoveHandler);
      this.mouseMoveHandler = null;
    }

    if (this.focusHandler) {
      document.removeEventListener('focusin', this.focusHandler);
      this.focusHandler = null;
    }
  }

  openPanel(): void {
    this.a11y.openPanel();
    setTimeout(() => this.focusFirstElement(), 100);
  }

  closePanel(): void {
    this.a11y.closePanel();
  }

  togglePanel(): void {
    this.a11y.togglePanel();
    if (this.a11y.panelOpen()) {
      setTimeout(() => this.focusFirstElement(), 100);
    }
  }

  private focusFirstElement(): void {
    const panel = this.elementRef.nativeElement.querySelector('.a11y-panel');
    if (panel) {
      const firstFocusable = panel.querySelector('button, [tabindex="0"]') as HTMLElement;
      if (firstFocusable) {
        firstFocusable.focus();
      }
    }
  }

  // Color Mode
  setColorMode(mode: ColorMode): void {
    this.a11y.setColorMode(mode);
  }

  // Profile
  setProfile(profile: AccessibilityProfile): void {
    this.a11y.setProfile(profile);
  }

  // Theme (compatibilidad)
  setTheme(theme: AccessibilityTheme): void {
    this.a11y.setTheme(theme);
  }

  // Font size
  increaseFontSize(): void {
    this.a11y.increaseFontSize();
  }

  decreaseFontSize(): void {
    this.a11y.decreaseFontSize();
  }

  // Toggles
  toggleHighlightLinks(): void {
    this.a11y.updateSetting('highlightLinks', !this.a11y.highlightLinks());
  }

  toggleReducedMotion(): void {
    this.a11y.updateSetting('reducedMotion', !this.a11y.reducedMotion());
  }

  toggleReadingGuide(): void {
    this.a11y.updateSetting('readingGuide', !this.a11y.readingGuide());
  }

  toggleLargeCursor(): void {
    this.a11y.updateSetting('largeCursor', !this.a11y.largeCursor());
  }

  toggleHighlightFocus(): void {
    this.a11y.updateSetting('highlightFocus', !this.a11y.highlightFocus());
  }

  // Reading Mode
  toggleReadingMode(): void {
    this.a11y.toggleReadingMode();
  }

  // Text to Speech
  toggleTextToSpeech(): void {
    this.a11y.toggleTextToSpeech();
  }

  speakSelection(): void {
    this.a11y.speakSelection();
  }

  speakMainContent(): void {
    this.a11y.speakMainContent();
  }

  stopSpeaking(): void {
    this.a11y.stopSpeaking();
  }

  setTTSRate(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.a11y.setTextToSpeechRate(parseFloat(target.value));
  }

  increaseTTSRate(): void {
    const currentRate = this.a11y.textToSpeechRate();
    this.a11y.setTextToSpeechRate(Math.min(2, currentRate + 0.25));
  }

  decreaseTTSRate(): void {
    const currentRate = this.a11y.textToSpeechRate();
    this.a11y.setTextToSpeechRate(Math.max(0.5, currentRate - 0.25));
  }

  // Reset
  resetSettings(): void {
    this.a11y.stopSpeaking();
    this.a11y.resetSettings();
  }
}
