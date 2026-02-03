import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PinLoginRequest } from '@models';
import { BaseVisualLoginComponent } from './base-visual-login.component';
import { AccessibilityPanelComponent } from '@components/accessibility-panel/accessibility-panel.component';
import { ButtonDirective } from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-pin-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonDirective,
    IconDirective,
    AccessibilityPanelComponent,
  ],
  templateUrl: './pin-login.component.html',
  styleUrl: './pin-login.component.scss',
})
export class PinLoginComponent extends BaseVisualLoginComponent {
  // Estado específico de PIN
  pin = '';
  maxPinLength = 4;
  rememberDevice = false;
  showKeyboardInput = false;
  audioFeedbackEnabled = false;

  pinPad = [
    ['1', '2', '3'],
    ['4', '5', '6'],
    ['7', '8', '9'],
    ['clear', '0', 'submit'],
  ];

  // ============================================
  // Manejo de PIN
  // ============================================

  onPinDigit(digit: string): void {
    if (this.isLoading || this.isLocked) return;

    if (this.pin.length < this.maxPinLength) {
      this.pin += digit;
      this.clearError();

      // Auto-submit when 4 digits entered
      if (this.pin.length === this.maxPinLength) {
        this.onSubmit();
      }
    }
  }

  onClear(): void {
    this.pin = '';
    this.clearError();
  }

  onBackspace(): void {
    if (this.pin.length > 0) {
      this.pin = this.pin.slice(0, -1);
    }
  }

  // ============================================
  // Submit (implementación requerida)
  // ============================================

  onSubmit(): void {
    if (this.pin.length !== this.maxPinLength || this.isLoading) return;

    this.isLoading = true;
    this.clearError();

    const request: PinLoginRequest = {
      userId: this.userId,
      pin: this.pin,
      deviceId: this.authService.getDeviceId(),
      rememberDevice: this.rememberDevice,
    };

    this.authService.loginWithPin(request).subscribe({
      next: (response) => {
        if (response.success && response.data?.success) {
          this.navigateToDashboard();
        } else {
          this.handleLoginResponseError(
            response.data,
            'PIN incorrecto',
            () => this.pin = ''
          );
        }
      },
      error: (error) => {
        this.handleHttpError(
          error,
          'Error al verificar el PIN',
          () => this.pin = ''
        );
      },
    });
  }

  // ============================================
  // UI Helpers
  // ============================================

  get pinDots(): boolean[] {
    return Array(this.maxPinLength)
      .fill(false)
      .map((_, i) => i < this.pin.length);
  }

  handlePadClick(key: string): void {
    if (key === 'clear') {
      this.onBackspace();
    } else if (key === 'submit') {
      this.onSubmit();
    } else {
      this.onPinDigit(key);
    }
  }

  getPadKeyLabel(key: string): string {
    if (key === 'clear') return '⌫';
    if (key === 'submit') return '✓';
    return key;
  }

  getPadKeyAriaLabel(key: string): string {
    if (key === 'clear') return 'Borrar último dígito';
    if (key === 'submit') return 'Confirmar PIN';
    return `Dígito ${key}`;
  }

  // ============================================
  // Teclado alternativo
  // ============================================

  toggleKeyboardInput(): void {
    this.showKeyboardInput = !this.showKeyboardInput;
    this.pin = '';
    this.clearError();
  }

  onPinTextInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    // Solo permitir números
    const numericValue = input.value.replace(/\D/g, '');
    this.pin = numericValue.slice(0, this.maxPinLength);
    input.value = this.pin;

    // Feedback de audio si está habilitado
    if (this.audioFeedbackEnabled && this.pin.length > 0) {
      this.playFeedbackSound('digit');
    }

    // No auto-submit con teclado para dar tiempo a revisar
  }

  // ============================================
  // Audio feedback
  // ============================================

  toggleAudioFeedback(): void {
    this.audioFeedbackEnabled = !this.audioFeedbackEnabled;
    if (this.audioFeedbackEnabled) {
      this.playFeedbackSound('success');
    }
  }

  private playFeedbackSound(type: 'digit' | 'success' | 'error'): void {
    if (!this.audioFeedbackEnabled) return;

    try {
      const audioContext = new (window.AudioContext || (window as any).webkitAudioContext)();
      const oscillator = audioContext.createOscillator();
      const gainNode = audioContext.createGain();

      oscillator.connect(gainNode);
      gainNode.connect(audioContext.destination);

      switch (type) {
        case 'digit':
          oscillator.frequency.value = 800;
          gainNode.gain.value = 0.1;
          break;
        case 'success':
          oscillator.frequency.value = 1200;
          gainNode.gain.value = 0.15;
          break;
        case 'error':
          oscillator.frequency.value = 300;
          gainNode.gain.value = 0.15;
          break;
      }

      oscillator.start();
      oscillator.stop(audioContext.currentTime + 0.1);
    } catch {
      // Audio no disponible, ignorar silenciosamente
    }
  }
}
