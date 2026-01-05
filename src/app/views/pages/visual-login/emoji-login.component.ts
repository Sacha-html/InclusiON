import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { EmojiLoginRequest } from '../../../models';
import { AccessibilityPanelComponent } from '../../../components/accessibility-panel/accessibility-panel.component';
import {
  ContainerComponent,
  RowComponent,
  ColComponent,
  CardComponent,
  CardBodyComponent,
  ButtonDirective,
  SpinnerComponent,
  AlertComponent,
  FormCheckComponent,
  FormCheckInputDirective,
  FormCheckLabelDirective,
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-emoji-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ContainerComponent,
    RowComponent,
    ColComponent,
    CardComponent,
    CardBodyComponent,
    ButtonDirective,
    SpinnerComponent,
    AlertComponent,
    FormCheckComponent,
    FormCheckInputDirective,
    FormCheckLabelDirective,
    IconDirective,
    AccessibilityPanelComponent,
  ],
  templateUrl: './emoji-login.component.html',
  styleUrl: './emoji-login.component.scss',
})
export class EmojiLoginComponent implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private authService = inject(AuthService);

  userId = '';
  displayName = '';
  initial = '';
  avatarColor = '#667eea';

  selectedEmojis: string[] = [];
  maxEmojiCount = 4;
  isLoading = false;
  errorMessage = '';
  remainingAttempts: number | null = null;
  isLocked = false;
  lockoutSeconds = 0;
  rememberDevice = false;

  // Emoji grid - common, recognizable emojis
  availableEmojis: string[] = [
    '😀', '😍', '🥳', '😎',
    '🐶', '🐱', '🐻', '🦁',
    '🌟', '🌈', '🌸', '🍀',
    '🍎', '🍕', '🎂', '🍦',
    '⚽', '🎨', '🎵', '🎮',
    '🚗', '✈️', '🏠', '🎁',
  ];

  ngOnInit(): void {
    const params = this.route.snapshot.queryParams;
    this.userId = params['userId'] || '';
    this.displayName = params['displayName'] || '';
    this.initial = params['initial'] || this.displayName.charAt(0).toUpperCase();
    this.avatarColor = params['avatarColor'] || '#667eea';

    if (!this.userId) {
      this.router.navigate(['/login']);
    }
  }

  selectEmoji(emoji: string): void {
    if (this.isLoading || this.isLocked) return;

    // Check if already selected
    const index = this.selectedEmojis.indexOf(emoji);
    if (index > -1) {
      // Remove emoji (toggle off)
      this.selectedEmojis.splice(index, 1);
    } else if (this.selectedEmojis.length < this.maxEmojiCount) {
      // Add emoji
      this.selectedEmojis.push(emoji);
      this.errorMessage = '';

      // Auto-submit when 4 emojis selected
      if (this.selectedEmojis.length === this.maxEmojiCount) {
        setTimeout(() => this.onSubmit(), 300);
      }
    }
  }

  isSelected(emoji: string): boolean {
    return this.selectedEmojis.includes(emoji);
  }

  getSelectionOrder(emoji: string): number {
    return this.selectedEmojis.indexOf(emoji) + 1;
  }

  onClear(): void {
    this.selectedEmojis = [];
    this.errorMessage = '';
  }

  onSubmit(): void {
    if (this.selectedEmojis.length !== this.maxEmojiCount || this.isLoading) return;

    this.isLoading = true;
    this.errorMessage = '';

    const request: EmojiLoginRequest = {
      userId: parseInt(this.userId, 10),
      emojiSequence: this.selectedEmojis,
      deviceId: this.authService.getDeviceId(),
      rememberDevice: this.rememberDevice,
    };

    this.authService.loginWithEmoji(request).subscribe({
      next: (response) => {
        if (response.success && response.data?.success) {
          this.router.navigate(['/dashboard']);
        } else {
          this.handleLoginError(response.data);
        }
      },
      error: (error) => {
        console.error('Emoji login error:', error);
        this.errorMessage = error.message || 'Error al verificar los emojis';
        this.selectedEmojis = [];
        this.isLoading = false;
      },
    });
  }

  private handleLoginError(data: any): void {
    this.selectedEmojis = [];
    this.isLoading = false;

    if (data?.isLocked) {
      this.isLocked = true;
      this.lockoutSeconds = data.lockoutSecondsRemaining || 60;
      this.startLockoutTimer();
      this.errorMessage = `Cuenta bloqueada. Espera ${this.lockoutSeconds} segundos.`;
    } else {
      this.remainingAttempts = data?.remainingAttempts || null;
      this.errorMessage = data?.errorMessage || 'Secuencia incorrecta';

      if (this.remainingAttempts !== null && this.remainingAttempts <= 2) {
        this.errorMessage += `. Te quedan ${this.remainingAttempts} intentos.`;
      }
    }
  }

  private startLockoutTimer(): void {
    const interval = setInterval(() => {
      this.lockoutSeconds--;
      if (this.lockoutSeconds <= 0) {
        clearInterval(interval);
        this.isLocked = false;
        this.errorMessage = '';
      }
    }, 1000);
  }

  goBack(): void {
    this.router.navigate(['/login/identify'], {
      queryParams: { userType: 'PERSON' },
    });
  }
}
