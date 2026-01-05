import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { ColorShapeLoginRequest } from '../../../models';
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

interface ColorShape {
  id: number;
  shape: 'circle' | 'square' | 'triangle' | 'star' | 'heart' | 'diamond';
  color: string;
  colorName: string;
  shapeName: string;
}

@Component({
  selector: 'app-color-shape-login',
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
  templateUrl: './color-shape-login.component.html',
  styleUrl: './color-shape-login.component.scss',
})
export class ColorShapeLoginComponent implements OnInit {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private authService = inject(AuthService);

  userId = '';
  displayName = '';
  initial = '';
  avatarColor = '#667eea';

  selectedShapeId: number | null = null;
  isLoading = false;
  errorMessage = '';
  remainingAttempts: number | null = null;
  isLocked = false;
  lockoutSeconds = 0;
  rememberDevice = false;

  // Color and shape combinations
  colorShapes: ColorShape[] = this.generateColorShapes();

  private generateColorShapes(): ColorShape[] {
    const colors = [
      { color: '#EF5350', name: 'rojo' },
      { color: '#42A5F5', name: 'azul' },
      { color: '#66BB6A', name: 'verde' },
      { color: '#FFA726', name: 'naranja' },
      { color: '#AB47BC', name: 'morado' },
      { color: '#FFEE58', name: 'amarillo' },
    ];

    const shapes: Array<{ shape: ColorShape['shape']; name: string }> = [
      { shape: 'circle', name: 'círculo' },
      { shape: 'square', name: 'cuadrado' },
      { shape: 'triangle', name: 'triángulo' },
      { shape: 'star', name: 'estrella' },
    ];

    const combinations: ColorShape[] = [];
    let id = 1;

    for (const colorObj of colors) {
      for (const shapeObj of shapes) {
        combinations.push({
          id: id++,
          shape: shapeObj.shape,
          color: colorObj.color,
          colorName: colorObj.name,
          shapeName: shapeObj.name,
        });
      }
    }

    return combinations;
  }

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

  selectShape(shape: ColorShape): void {
    if (this.isLoading || this.isLocked) return;

    this.selectedShapeId = shape.id;
    this.errorMessage = '';
  }

  isSelected(shape: ColorShape): boolean {
    return this.selectedShapeId === shape.id;
  }

  onSubmit(): void {
    if (this.selectedShapeId === null || this.isLoading) return;

    this.isLoading = true;
    this.errorMessage = '';

    const request: ColorShapeLoginRequest = {
      userId: parseInt(this.userId, 10),
      colorShapeId: this.selectedShapeId,
      deviceId: this.authService.getDeviceId(),
      rememberDevice: this.rememberDevice,
    };

    this.authService.loginWithColorShape(request).subscribe({
      next: (response) => {
        if (response.success && response.data?.success) {
          this.router.navigate(['/dashboard']);
        } else {
          this.handleLoginError(response.data);
        }
      },
      error: (error) => {
        console.error('Color-shape login error:', error);
        this.errorMessage = error.message || 'Error al verificar';
        this.selectedShapeId = null;
        this.isLoading = false;
      },
    });
  }

  private handleLoginError(data: any): void {
    this.selectedShapeId = null;
    this.isLoading = false;

    if (data?.isLocked) {
      this.isLocked = true;
      this.lockoutSeconds = data.lockoutSecondsRemaining || 60;
      this.startLockoutTimer();
      this.errorMessage = `Cuenta bloqueada. Espera ${this.lockoutSeconds} segundos.`;
    } else {
      this.remainingAttempts = data?.remainingAttempts || null;
      this.errorMessage = data?.errorMessage || 'Selección incorrecta';

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

  getShapeAriaLabel(shape: ColorShape): string {
    return `${shape.shapeName} ${shape.colorName}${this.isSelected(shape) ? ', seleccionado' : ''}`;
  }

  goBack(): void {
    this.router.navigate(['/login/identify'], {
      queryParams: { userType: 'PERSON' },
    });
  }
}
