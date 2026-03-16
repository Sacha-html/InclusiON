import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ButtonDirective } from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';
import { AccessibilityPanelComponent } from '@components/accessibility-panel/accessibility-panel.component';

export type UserRole = 'PERSON' | 'PROFESSIONAL' | 'FAMILY';

interface RoleOption {
  type: UserRole;
  title: string;
  description: string;
  icon: string;
  color: string;
  bgColor: string;
}

@Component({
  selector: 'app-role-selection',
  standalone: true,
  imports: [
    CommonModule,
    ButtonDirective,
    IconDirective,
    AccessibilityPanelComponent,
  ],
  templateUrl: './role-selection.component.html',
  styleUrl: './role-selection.component.scss',
})
export class RoleSelectionComponent {
  private router = inject(Router);

  roles: RoleOption[] = [
    {
      type: 'PERSON',
      title: 'Soy una Persona',
      description: 'Ingresa con tu cuenta personal',
      icon: 'cilUser',
      color: 'var(--a11y-role-person, #4CAF50)',
      bgColor: 'var(--a11y-role-person-bg, #E8F5E9)',
    },
    {
      type: 'PROFESSIONAL',
      title: 'Soy Profesional',
      description: 'Acceso para profesionales de apoyo',
      icon: 'cilMedicalCross',
      color: 'var(--a11y-role-professional, #2196F3)',
      bgColor: 'var(--a11y-role-professional-bg, #E3F2FD)',
    },
    {
      type: 'FAMILY',
      title: 'Soy Familia',
      description: 'Acceso para familiares o tutores',
      icon: 'cilPeople',
      color: 'var(--a11y-role-family, #9C27B0)',
      bgColor: 'var(--a11y-role-family-bg, #F3E5F5)',
    },
  ];

  selectRole(role: UserRole): void {
    this.router.navigate(['/login/identify'], {
      queryParams: { userType: role },
    });
  }

  goToAdminLogin(): void {
    this.router.navigate(['/admin-login']);
  }
}
