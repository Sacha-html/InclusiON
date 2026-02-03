import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

/**
 * Claves de localStorage utilizadas en la aplicación.
 * Centraliza todas las claves para evitar errores de tipeo.
 */
export const STORAGE_KEYS = {
  ACCESS_TOKEN: 'access_token',
  REFRESH_TOKEN: 'refresh_token',
  CURRENT_USER: 'current_user',
  DEVICE_ID: 'device_id',
  ACCESSIBILITY_SETTINGS: 'a11y-settings',
  ACCESSIBILITY_PREFERENCES: 'accessibility_preferences',
} as const;

export type StorageKey = typeof STORAGE_KEYS[keyof typeof STORAGE_KEYS];

/**
 * Servicio centralizado para operaciones de localStorage.
 * Proporciona tipado, manejo de errores y compatibilidad con SSR.
 */
@Injectable({
  providedIn: 'root'
})
export class LocalStorageService {
  private readonly platformId = inject(PLATFORM_ID);

  /**
   * Verifica si localStorage está disponible (no SSR)
   */
  private get isAvailable(): boolean {
    return isPlatformBrowser(this.platformId);
  }

  // ============================================
  // Métodos genéricos
  // ============================================

  /**
   * Obtiene un valor string del localStorage
   */
  get(key: StorageKey): string | null {
    if (!this.isAvailable) return null;

    try {
      return localStorage.getItem(key);
    } catch {
      return null;
    }
  }

  /**
   * Guarda un valor string en localStorage
   */
  set(key: StorageKey, value: string): boolean {
    if (!this.isAvailable) return false;

    try {
      localStorage.setItem(key, value);
      return true;
    } catch (error) {
      // QuotaExceededError u otros errores
      console.warn(`LocalStorage: Error al guardar '${key}'`, error);
      return false;
    }
  }

  /**
   * Obtiene y parsea un objeto JSON del localStorage
   */
  getObject<T>(key: StorageKey): T | null {
    const value = this.get(key);
    if (!value) return null;

    try {
      return JSON.parse(value) as T;
    } catch {
      return null;
    }
  }

  /**
   * Guarda un objeto como JSON en localStorage
   */
  setObject<T>(key: StorageKey, value: T): boolean {
    try {
      return this.set(key, JSON.stringify(value));
    } catch {
      return false;
    }
  }

  /**
   * Elimina una clave del localStorage
   */
  remove(key: StorageKey): void {
    if (!this.isAvailable) return;

    try {
      localStorage.removeItem(key);
    } catch {
      // Ignorar errores
    }
  }

  /**
   * Elimina múltiples claves del localStorage
   */
  removeMany(keys: StorageKey[]): void {
    keys.forEach(key => this.remove(key));
  }

  /**
   * Verifica si una clave existe en localStorage
   */
  has(key: StorageKey): boolean {
    return this.get(key) !== null;
  }

  // ============================================
  // Métodos específicos de autenticación
  // ============================================

  /**
   * Obtiene el access token
   */
  getAccessToken(): string | null {
    return this.get(STORAGE_KEYS.ACCESS_TOKEN);
  }

  /**
   * Guarda el access token
   */
  setAccessToken(token: string): boolean {
    return this.set(STORAGE_KEYS.ACCESS_TOKEN, token);
  }

  /**
   * Obtiene el refresh token
   */
  getRefreshToken(): string | null {
    return this.get(STORAGE_KEYS.REFRESH_TOKEN);
  }

  /**
   * Guarda el refresh token
   */
  setRefreshToken(token: string): boolean {
    return this.set(STORAGE_KEYS.REFRESH_TOKEN, token);
  }

  /**
   * Obtiene el usuario actual
   */
  getCurrentUser<T>(): T | null {
    return this.getObject<T>(STORAGE_KEYS.CURRENT_USER);
  }

  /**
   * Guarda el usuario actual
   */
  setCurrentUser<T>(user: T): boolean {
    return this.setObject(STORAGE_KEYS.CURRENT_USER, user);
  }

  /**
   * Limpia todos los datos de sesión
   */
  clearSession(): void {
    this.removeMany([
      STORAGE_KEYS.ACCESS_TOKEN,
      STORAGE_KEYS.REFRESH_TOKEN,
      STORAGE_KEYS.CURRENT_USER
    ]);
  }

  /**
   * Verifica si hay una sesión activa (tiene access token)
   */
  hasSession(): boolean {
    return this.has(STORAGE_KEYS.ACCESS_TOKEN);
  }

  // ============================================
  // Métodos específicos de dispositivo
  // ============================================

  /**
   * Obtiene o genera el device ID
   */
  getOrCreateDeviceId(): string {
    let deviceId = this.get(STORAGE_KEYS.DEVICE_ID);

    if (!deviceId) {
      deviceId = this.generateDeviceId();
      this.set(STORAGE_KEYS.DEVICE_ID, deviceId);
    }

    return deviceId;
  }

  /**
   * Genera un ID único para el dispositivo
   */
  private generateDeviceId(): string {
    const array = new Uint8Array(16);
    crypto.getRandomValues(array);
    return Array.from(array, byte => byte.toString(16).padStart(2, '0')).join('');
  }

  // ============================================
  // Métodos específicos de accesibilidad
  // ============================================

  /**
   * Obtiene las preferencias de accesibilidad
   */
  getAccessibilitySettings<T>(): T | null {
    return this.getObject<T>(STORAGE_KEYS.ACCESSIBILITY_SETTINGS);
  }

  /**
   * Guarda las preferencias de accesibilidad
   */
  setAccessibilitySettings<T>(settings: T): boolean {
    return this.setObject(STORAGE_KEYS.ACCESSIBILITY_SETTINGS, settings);
  }
}
