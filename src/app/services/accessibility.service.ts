import { Injectable, signal, effect, computed } from '@angular/core';

// Separamos modo de color de perfil de accesibilidad
export type ColorMode = 'light' | 'dark';
export type AccessibilityProfile = 'default' | 'high-contrast' | 'dyslexia' | 'low-vision';
export type FontSize = 'small' | 'medium' | 'large' | 'x-large';
export type LineSpacing = 'normal' | 'relaxed' | 'loose';
export type LetterSpacing = 'normal' | 'wide' | 'wider';

// Mantener compatibilidad con código existente
export type AccessibilityTheme = AccessibilityProfile;

export interface ColorModeOption {
  id: ColorMode;
  name: string;
  icon: string;
}

export interface ProfileOption {
  id: AccessibilityProfile;
  name: string;
  description: string;
  icon: string;
}

// Mantener compatibilidad
export interface ThemeOption extends ProfileOption {}

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
  colorMode: ColorMode;
  profile: AccessibilityProfile;
  fontSize: FontSize;
  lineSpacing: LineSpacing;
  letterSpacing: LetterSpacing;
  highlightLinks: boolean;
  highlightFocus: boolean;
  reducedMotion: boolean;
  readingGuide: boolean;
  largeCursor: boolean;
}

const DEFAULT_SETTINGS: AccessibilitySettings = {
  colorMode: 'light',
  profile: 'default',
  fontSize: 'medium',
  lineSpacing: 'normal',
  letterSpacing: 'normal',
  highlightLinks: false,
  highlightFocus: false,
  reducedMotion: false,
  readingGuide: false,
  largeCursor: false
};

@Injectable({
  providedIn: 'root'
})
export class AccessibilityService {
  private readonly STORAGE_KEY = 'a11y-settings';

  // Opciones de modo de color (claro/oscuro)
  readonly colorModes: ColorModeOption[] = [
    {
      id: 'light',
      name: 'Claro',
      icon: 'cilSun'
    },
    {
      id: 'dark',
      name: 'Oscuro',
      icon: 'cilMoon'
    }
  ];

  // Perfiles de accesibilidad (independientes del modo de color)
  readonly profiles: ProfileOption[] = [
    {
      id: 'default',
      name: 'Estándar',
      description: 'Sin ajustes especiales',
      icon: 'cilUser'
    },
    {
      id: 'high-contrast',
      name: 'Alto Contraste',
      description: 'Máximo contraste para baja visión',
      icon: 'cilContrast'
    },
    {
      id: 'dyslexia',
      name: 'Lectura Fácil',
      description: 'Fuente y espaciado optimizados',
      icon: 'cilNotes'
    },
    {
      id: 'low-vision',
      name: 'Visión Reducida',
      description: 'Texto e iconos más grandes',
      icon: 'cilZoomIn'
    }
  ];

  // Mantener compatibilidad con código existente
  readonly themes: ThemeOption[] = this.profiles;

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
  readonly colorMode = computed(() => this.settings().colorMode);
  readonly profile = computed(() => this.settings().profile);
  readonly currentTheme = computed(() => this.settings().profile); // Compatibilidad
  readonly fontSize = computed(() => this.settings().fontSize);
  readonly lineSpacing = computed(() => this.settings().lineSpacing);
  readonly letterSpacing = computed(() => this.settings().letterSpacing);
  readonly highlightLinks = computed(() => this.settings().highlightLinks);
  readonly highlightFocus = computed(() => this.settings().highlightFocus);
  readonly reducedMotion = computed(() => this.settings().reducedMotion);
  readonly readingGuide = computed(() => this.settings().readingGuide);
  readonly largeCursor = computed(() => this.settings().largeCursor);

  // Computed para saber si es modo oscuro
  readonly isDarkMode = computed(() => this.settings().colorMode === 'dark');

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
   * Cambia el modo de color (claro/oscuro)
   */
  setColorMode(mode: ColorMode): void {
    this.updateSetting('colorMode', mode);
  }

  /**
   * Alterna entre modo claro y oscuro
   */
  toggleColorMode(): void {
    this.setColorMode(this.colorMode() === 'light' ? 'dark' : 'light');
  }

  /**
   * Cambia el perfil de accesibilidad
   */
  setProfile(profile: AccessibilityProfile): void {
    this.updateSetting('profile', profile);
  }

  /**
   * Cambia el tema de accesibilidad (compatibilidad)
   */
  setTheme(theme: AccessibilityTheme): void {
    this.updateSetting('profile', theme);
  }

  /**
   * Obtiene el tema actual (compatibilidad)
   */
  getTheme(): AccessibilityTheme {
    return this.currentTheme();
  }

  /**
   * Obtiene la información del perfil actual
   */
  getCurrentProfileInfo(): ProfileOption {
    return this.profiles.find(p => p.id === this.profile()) || this.profiles[0];
  }

  /**
   * Obtiene la información del tema actual (compatibilidad)
   */
  getCurrentThemeInfo(): ThemeOption {
    return this.getCurrentProfileInfo();
  }

  /**
   * Cicla al siguiente perfil
   */
  cycleProfile(): void {
    const currentIndex = this.profiles.findIndex(p => p.id === this.profile());
    const nextIndex = (currentIndex + 1) % this.profiles.length;
    this.setProfile(this.profiles[nextIndex].id);
  }

  /**
   * Cicla al siguiente tema (compatibilidad)
   */
  cycleTheme(): void {
    this.cycleProfile();
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

    // Modo de color (claro/oscuro)
    root.setAttribute('data-color-mode', settings.colorMode);
    body.setAttribute('data-color-mode', settings.colorMode);

    // Clase para CoreUI dark mode
    if (settings.colorMode === 'dark') {
      root.classList.add('dark-theme');
      body.classList.add('dark-theme');
    } else {
      root.classList.remove('dark-theme');
      body.classList.remove('dark-theme');
    }

    // Perfil de accesibilidad
    root.setAttribute('data-profile', settings.profile);
    body.setAttribute('data-profile', settings.profile);

    // Mantener data-theme para compatibilidad con CSS existente
    root.setAttribute('data-theme', settings.profile);
    body.setAttribute('data-theme', settings.profile);

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
    body.classList.toggle('a11y-large-cursor', settings.largeCursor);
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
      case 'colorMode':
        const modeInfo = this.colorModes.find(m => m.id === value);
        return `Modo de color: ${modeInfo?.name}`;
      case 'profile':
        const profileInfo = this.profiles.find(p => p.id === value);
        return `Perfil de accesibilidad: ${profileInfo?.name}`;
      case 'theme': // Compatibilidad
        const themeInfo = this.themes.find(t => t.id === value);
        return `Perfil cambiado a: ${themeInfo?.name}`;
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
      case 'largeCursor':
        return value ? 'Cursor grande activado' : 'Cursor grande desactivado';
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
