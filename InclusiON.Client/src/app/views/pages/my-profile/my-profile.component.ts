import { Component, inject } from '@angular/core';
import { AuthService } from '@services';
import {
  BadgeComponent,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  ContainerComponent,
  RowComponent,
} from '@coreui/angular';

const ROLE_LABELS: Record<string, string> = {
  admin: 'Administrador',
  professional: 'Profesional',
  familyrepresentative: 'Familiar',
  personwithdisability: 'Persona',
};

@Component({
  selector: 'app-my-profile',
  imports: [
    ContainerComponent,
    RowComponent,
    ColComponent,
    CardComponent,
    CardHeaderComponent,
    CardBodyComponent,
    BadgeComponent,
  ],
  templateUrl: './my-profile.component.html',
})
export class MyProfileComponent {
  private readonly authService = inject(AuthService);

  // El usuario autenticado ya está disponible localmente (seteado al loguear);
  // no hay endpoint de perfil en el backend, así que se muestra con estos datos.
  user = this.authService.getCurrentUser();

  roleLabel(role: string | undefined): string {
    if (!role) return '-';
    return ROLE_LABELS[role.toLowerCase()] ?? role;
  }
}
