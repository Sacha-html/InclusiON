import { Injectable, signal, effect, computed } from '@angular/core';

export type AccessibilityTheme = 'default' | 'high-contrast' | 'dyslexia' | 'low-vision';
export type FontSize = 'small' | 'medium' | 'large' | 'x-large';
export type LineSpacing = 'normal' | 'relaxed' | 'loose';
export type LetterSpacing = 'normal' | 'wide' | 'wider';

export interface ThemeOption {
  id: AccessibilityTheme;
  name: string;
  description: string;
  icon: string;
}

export interface FontSizeOption {
  id: FontSize;
  name: string;
  value: string;
}

export interface SpacingOption {
  id: LineSpacing | LetterSpacing;
  name: string;
  value: string;
}

export interface AccessibilitySettings {
  theme: AccessibilityTheme;
  fontSize: FontSize;
  lineSpacing: LineSpacing;
  letterSpacing: LetterSpacing;
  highlightLinks: boolean;
  highlightFocus: boolean;
  reducedMotion: boolean;
  readingGuide: boolean;
}

const DEFAULT_SETTINGS: AccessibilitySettings = {
  theme: 'default',
  fontSize: 'medium',
  lineSpacing: 'normal',
  letterSpacing: 'normal',
  highlightLinks: false,
  highlightFocus: false,
  reducedMotion: false,
  readingGuide: false
};

@Injectable({
  providedIn: 'root'
})
export class AccessibilityService {
  private readonly STORAGE_KEY = 'a11y-settings';

  readonly themes: ThemeOption[] = [
    {
      id: 'default',
      name: 'Estándar',
      description: 'Tema por defecto con buena legibilidad',
      icon: 'cilSun'
    },
    {
      id: 'high-contrast',
      name: 'Alto Contraste',
      description: 'Máximo contraste para baja visión severa',
      icon: 'cilContrast'
    },
    {
      id: 'dyslexia',
      name: 'Dislexia',
      description: 'Fuente y espaciado optimizados para dislexia',
      icon: 'cilNotes'
    },
    {
      id: 'low-vision',
      name: 'Visión Reducida',
      description: 'Texto e iconos grandes para baja visión',
      icon: 'cilZoomIn'
    }
  ];

  readonly fontSizes: FontSizeOption[] = [
    { id: 'small', name: 'Pequeño', value: '14px' },
    { id: 'medium', name: 'Mediano', value: '16px' },
    { id: 'large', name: 'Grande', value: '18px' },
    { id: 'x-large', name: 'Muy Grande', value: '20px' }
  ];

  readonly lineSpacings: SpacingOption[] = [
    { id: 'normal', name: 'Normal', value: '1.5' },
    { id: 'relaxed', name: 'Relajado', value: '1.75' },
    { id: 'loose', name: 'Amplio', value: '2' }
  ];

  readonly letterSpacings: SpacingOption[] = [
    { id: 'normal', name: 'Normal', value: '0.01em' },
    { id: 'wide', name: 'Amplio', value: '0.05em' },
    { id: 'wider', name: 'Muy Amplio', value: '0.1em' }
  ];

  // Signals para cada configuración
  readonly settings = signal<AccessibilitySettings>(this.loadSettings());

  // Computed signals para acceso individual
  readonly currentTheme = computed(() => this.settings().theme);
  readonly fontSize = computed(() => this.settings().fontSize);
  readonly lineSpacing = computed(() => this.settings().lineSpacing);
  readonly letterSpacing = computed(() => this.settings().letterSpacing);
  readonly highlightLinks = computed(() => this.settings().highlightLinks);
  readonly highlightFocus = computed(() => this.settings().highlightFocus);
  readonly reducedMotion = computed(() => this.settings().reducedMotion);
  readonly readingGuide = computed(() => this.settings().readingGuide);

  // Signal para el panel abierto/cerrado
  readonly panelOpen = signal(false);

  constructor() {
    // Efecto que aplica todas las configuraciones cuando cambian
    effect(() => {
      this.applySettings(this.settings());
    });

    // Aplicar configuración inicial
    this.applySettings(this.settings());
  }

  /**
   * Actualiza una configuración específica
   */
  updateSetting<K extends keyof AccessibilitySettings>(key: K, value: AccessibilitySettings[K]): void {
    this.settings.update(current => ({
      ...current,
      [key]: value
    }));
    this.saveSettings();
    this.announce(this.getAnnouncementForSetting(key, value));
  }

  /**
   * Cambia el tema de accesibilidad
   */
  setTheme(theme: AccessibilityTheme): void {
    this.updateSetting('theme', theme);
  }

  /**
   * Obtiene el tema actual
   */
  getTheme(): AccessibilityTheme {
    return this.currentTheme();
  }

  /**
   * Obtiene la información del tema actual
   */
  getCurrentThemeInfo(): ThemeOption {
    return this.themes.find(t => t.id === this.currentTheme()) || this.themes[0];
  }

  /**
   * Cicla al siguiente tema
   */
  cycleTheme(): void {
    const currentIndex = this.themes.findIndex(t => t.id === this.currentTheme());
    const nextIndex = (currentIndex + 1) % this.themes.length;
    this.setTheme(this.themes[nextIndex].id);
  }

  /**
   * Aumenta el tamaño de fuente
   */
  increaseFontSize(): void {
    const currentIndex = this.fontSizes.findIndex(f => f.id === this.fontSize());
    if (currentIndex < this.fontSizes.length - 1) {
      this.updateSetting('fontSize', this.fontSizes[currentIndex + 1].id);
    }
  }

  /**
   * Disminuye el tamaño de fuente
   */
  decreaseFontSize(): void {
    const currentIndex = this.fontSizes.findIndex(f => f.id === this.fontSize());
    if (currentIndex > 0) {
      this.updateSetting('fontSize', this.fontSizes[currentIndex - 1].id);
    }
  }

  /**
   * Restablece todas las configuraciones a los valores por defecto
   */
  resetSettings(): void {
    this.settings.set({ ...DEFAULT_SETTINGS });
    this.saveSettings();
    this.announce('Configuraciones de accesibilidad restablecidas a valores por defecto');
  }

  /**
   * Abre/cierra el panel de accesibilidad
   */
  togglePanel(): void {
    this.panelOpen.update(open => !open);
  }

  /**
   * Abre el panel
   */
  openPanel(): void {
    this.panelOpen.set(true);
  }

  /**
   * Cierra el panel
   */
  closePanel(): void {
    this.panelOpen.set(false);
  }

  /**
   * Aplica todas las configuraciones al documento
   */
  private applySettings(settings: AccessibilitySettings): void {
    const root = document.documentElement;
    const body = document.body;

    // Tema
    root.setAttribute('data-theme', settings.theme);
    body.setAttribute('data-theme', settings.theme);

    // Tamaño de fuente
    const fontSizeOption = this.fontSizes.find(f => f.id === settings.fontSize);
    if (fontSizeOption) {
      root.style.setProperty('--a11y-font-size-override', fontSizeOption.value);
    }

    // Espaciado de líneas
    const lineSpacingOption = this.lineSpacings.find(l => l.id === settings.lineSpacing);
    if (lineSpacingOption) {
      root.style.setProperty('--a11y-line-height-override', lineSpacingOption.value);
    }

    // Espaciado de letras
    const letterSpacingOption = this.letterSpacings.find(l => l.id === settings.letterSpacing);
    if (letterSpacingOption) {
      root.style.setProperty('--a11y-letter-spacing-override', letterSpacingOption.value);
    }

    // Clases de utilidad
    body.classList.toggle('a11y-highlight-links', settings.highlightLinks);
    body.classList.toggle('a11y-highlight-focus', settings.highlightFocus);
    body.classList.toggle('a11y-reduced-motion', settings.reducedMotion);
    body.classList.toggle('a11y-reading-guide', settings.readingGuide);
  }

  /**
   * Anuncia cambios para lectores de pantalla
   */
  private announce(message: string): void {
    const announcement = document.createElement('div');
    announcement.setAttribute('role', 'status');
    announcement.setAttribute('aria-live', 'polite');
    announcement.setAttribute('aria-atomic', 'true');
    announcement.className = 'visually-hidden';
    announcement.textContent = message;

    document.body.appendChild(announcement);

    setTimeout(() => {
      document.body.removeChild(announcement);
    }, 1000);
  }

  /**
   * Genera mensaje de anuncio para cada configuración
   */
  private getAnnouncementForSetting(key: string, value: unknown): string {
    switch (key) {
      case 'theme':
        const themeInfo = this.themes.find(t => t.id === value);
        return `Tema cambiado a: ${themeInfo?.name}`;
      case 'fontSize':
        const fontInfo = this.fontSizes.find(f => f.id === value);
        return `Tamaño de fuente: ${fontInfo?.name}`;
      case 'lineSpacing':
        const lineInfo = this.lineSpacings.find(l => l.id === value);
        return `Espaciado de líneas: ${lineInfo?.name}`;
      case 'letterSpacing':
        const letterInfo = this.letterSpacings.find(l => l.id === value);
        return `Espaciado de letras: ${letterInfo?.name}`;
      case 'highlightLinks':
        return value ? 'Enlaces resaltados activado' : 'Enlaces resaltados desactivado';
      case 'highlightFocus':
        return value ? 'Foco mejorado activado' : 'Foco mejorado desactivado';
      case 'reducedMotion':
        return value ? 'Movimiento reducido activado' : 'Movimiento reducido desactivado';
      case 'readingGuide':
        return value ? 'Guía de lectura activada' : 'Guía de lectura desactivada';
      default:
        return 'Configuración actualizada';
    }
  }

  /**
   * Carga las configuraciones guardadas en localStorage
   */
  private loadSettings(): AccessibilitySettings {
    if (typeof localStorage !== 'undefined') {
      const saved = localStorage.getItem(this.STORAGE_KEY);
      if (saved) {
        try {
          const parsed = JSON.parse(saved);
          return { ...DEFAULT_SETTINGS, ...parsed };
        } catch {
          return { ...DEFAULT_SETTINGS };
        }
      }
    }
    return { ...DEFAULT_SETTINGS };
  }

  /**
   * Guarda las configuraciones en localStorage
   */
  private saveSettings(): void {
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(this.settings()));
    }
  }
}
