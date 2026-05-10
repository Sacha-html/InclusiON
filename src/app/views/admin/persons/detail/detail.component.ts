import { Component, inject, OnInit } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { CatalogsService, PersonsService, ToastService, FamilyService, AuthService } from '@services';
import { Permissions } from '@shared/constants/permissions';
import { AppRoutes } from '@shared/constants/app-routes';
import { PersonResponse, PersonSkillProfileResponse, SkillAreaItem, PersonRepresentativeResponse, FamilyResponse, PersonRepresentativeHistoryResponse } from '@models';
import { ConfirmModalComponent } from '@shared/components/confirm-modal/confirm-modal.component';
import { PersonBasicInfoComponent } from './components/person-basic-info.component';
import { PersonSkillsComponent } from './components/person-skills.component';
import { PersonLinksComponent } from './components/person-links.component';
import { AdminDiagnosesComponent } from './components/admin-diagnoses.component';
import { AdminPersonReportsComponent } from './components/admin-person-reports.component';
import { PersonAccessibilityComponent } from './components/person-accessibility.component';
import { BadgeComponent, CardBodyComponent, CardComponent, CardHeaderComponent } from '@coreui/angular';

@Component({
  selector: 'app-detail',
  standalone: true,
  imports: [
    BadgeComponent,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    ConfirmModalComponent,
    PersonBasicInfoComponent,
    PersonSkillsComponent,
    PersonLinksComponent,
    AdminDiagnosesComponent,
    AdminPersonReportsComponent,
    PersonAccessibilityComponent,
  ],
  templateUrl: './detail.component.html',
  styleUrl: './detail.component.scss',
})
export class DetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly personsService = inject(PersonsService);
  private readonly catalogsService = inject(CatalogsService);
  private readonly toastService = inject(ToastService);
  private readonly familyService = inject(FamilyService);
  private readonly authService = inject(AuthService);

  person: PersonResponse | null = null;
  showDeactivateModal = false;
  activeTab = 'datos';

  // Skill profile
  skillProfile: PersonSkillProfileResponse[] = [];
  allSkillAreas: SkillAreaItem[] = [];
  showAddSkillAreaModal = false;
  selectedSkillAreaIds: Set<number> = new Set();
  skillAreaError = '';
  skillAreaLoading = false;

  // Family links
  representatives: PersonRepresentativeResponse[] = [];
  loadingRepresentatives = false;
  showLinkModal = false;
  linkingFamily = false;
  linkFamilyError = '';
  linkRelationship = '';
  linkIsPrimary = false;
  showUnlinkModal = false;
  unlinkingRepresentative: PersonRepresentativeResponse | null = null;
  unlinkObservation = '';
  unlinking = false;

  // History
  showHistoryModal = false;
  linkHistory: PersonRepresentativeHistoryResponse[] = [];
  loadingHistory = false;

  readonly searchFamilyFn = (query: string): Observable<FamilyResponse[]> => {
    const linkedIds = new Set(this.representatives.filter(r => r.isActive).map(r => r.representativeId));
    return this.familyService.getAvailableFamilies(query || undefined).pipe(
      map((data: FamilyResponse[]) => (data ?? []).filter((f: FamilyResponse) => !linkedIds.has(f.id)))
    );
  };

  canLink = this.authService.hasPermission(Permissions.Family.Link);
  canUnlink = this.authService.hasPermission(Permissions.Family.Unlink);
  canViewHistory = this.authService.hasPermission(Permissions.Family.Read);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.personsService.getPersonById(id).subscribe({
        next: (person) => {
          this.person = person;
          this.loadSkillProfile();
          this.loadRepresentatives();
        },
        error: () => this.router.navigate([AppRoutes.Admin.Persons]),
      });
    }
  }

  loadSkillProfile(): void {
    if (!this.person) return;
    this.personsService.getSkillProfile(this.person.id).subscribe({
      next: (data) => (this.skillProfile = data ?? []),
      error: () => this.toastService.error('Error al cargar el perfil de habilidades'),
    });
  }

  loadSkillAreas(): void {
    this.catalogsService.getSkillAreas().subscribe({
      next: (areas) => {
        const activeIds = new Set(this.skillProfile.filter(sp => sp.isActive).map(sp => sp.skillAreaId));
        this.allSkillAreas = (areas ?? []).filter(a => !activeIds.has(a.id));
        this.showAddSkillAreaModal = true;
      },
      error: () => this.toastService.error('Error al cargar las áreas de habilidad'),
    });
  }

  closeAddSkillAreaModal(): void {
    this.showAddSkillAreaModal = false;
    this.skillAreaError = '';
  }

  confirmAddSkillAreas(): void {
    if (!this.person || this.selectedSkillAreaIds.size === 0) return;
    this.skillAreaLoading = true;
    this.skillAreaError = '';
    const ids = Array.from(this.selectedSkillAreaIds);
    let completed = 0;
    let errors = 0;

    for (const areaId of ids) {
      this.personsService.addSkillArea(this.person.id, areaId).subscribe({
        next: () => {
          completed++;
          if (completed + errors === ids.length) {
            this.skillAreaLoading = false;
            this.showAddSkillAreaModal = false;
            this.loadSkillProfile();
          }
        },
        error: () => {
          errors++;
          if (completed + errors === ids.length) {
            this.skillAreaLoading = false;
            this.skillAreaError = `${errors} área(s) no se pudieron agregar.`;
            if (completed > 0) {
              this.loadSkillProfile();
            }
          }
        },
      });
    }
  }

  toggleSkillArea(id: number): void {
    if (this.selectedSkillAreaIds.has(id)) {
      this.selectedSkillAreaIds.delete(id);
    } else {
      this.selectedSkillAreaIds.add(id);
    }
  }

  deactivateSkillArea(areaId: number): void {
    if (!this.person) return;
    this.personsService.deactivateSkillArea(this.person.id, areaId).subscribe({
      next: () => this.loadSkillProfile(),
      error: () => this.toastService.error('Error al desactivar el área de habilidad'),
    });
  }

  confirmDeactivate(): void {
    if (!this.person) return;
    this.personsService.deactivatePerson(this.person.id).subscribe({
      next: () => {
        this.toastService.success('Persona desactivada exitosamente');
        this.showDeactivateModal = false;
        this.person!.isActive = false;
      },
      error: () => {
        this.toastService.error('Error al desactivar la persona');
        this.showDeactivateModal = false;
      },
    });
  }

  // Family links methods
  loadRepresentatives(): void {
    if (!this.person) return;
    this.loadingRepresentatives = true;
    this.familyService.getPersonRepresentatives(this.person.id).subscribe({
      next: (data) => {
        this.representatives = data ?? [];
        this.loadingRepresentatives = false;
      },
      error: () => {
        this.loadingRepresentatives = false;
        this.toastService.error('Error al cargar familiares vinculados');
      },
    });
  }

  openLinkModal(): void {
    this.showLinkModal = true;
    this.linkRelationship = '';
    this.linkIsPrimary = false;
    this.linkFamilyError = '';
  }

  closeLinkModal(): void {
    this.showLinkModal = false;
  }

  openUnlinkModal(rep: PersonRepresentativeResponse): void {
    this.unlinkingRepresentative = rep;
    this.unlinkObservation = '';
    this.showUnlinkModal = true;
  }

  closeUnlinkModal(): void {
    this.showUnlinkModal = false;
    this.unlinkingRepresentative = null;
    this.unlinkObservation = '';
  }

  confirmUnlink(): void {
    if (!this.person || !this.unlinkingRepresentative || !this.unlinkObservation.trim()) return;
    this.unlinking = true;

    this.familyService.unlinkFamilyFromPerson(
      this.unlinkingRepresentative.representativeId,
      this.person.id,
      this.unlinkObservation
    ).subscribe({
      next: () => {
        this.unlinking = false;
        this.toastService.success('Familiar desvinculado exitosamente');
        this.showUnlinkModal = false;
        this.loadRepresentatives();
      },
      error: () => {
        this.unlinking = false;
        this.toastService.error('Error al desvincular el familiar');
      },
    });
  }

  // Child component handlers
  confirmAddSkillAreasFromChild(ids: number[]): void {
    if (!this.person || ids.length === 0) return;
    this.skillAreaLoading = true;
    this.skillAreaError = '';
    let completed = 0;
    let errors = 0;

    for (const areaId of ids) {
      this.personsService.addSkillArea(this.person.id, areaId).subscribe({
        next: () => {
          completed++;
          if (completed + errors === ids.length) {
            this.skillAreaLoading = false;
            this.showAddSkillAreaModal = false;
            this.loadSkillProfile();
          }
        },
        error: () => {
          errors++;
          if (completed + errors === ids.length) {
            this.skillAreaLoading = false;
            this.skillAreaError = `${errors} área(s) no se pudieron agregar.`;
            if (completed > 0) {
              this.loadSkillProfile();
            }
          }
        },
      });
    }
  }

  confirmLinkFromChild(data: { familyId: string; relationship: string; isPrimary: boolean }): void {
    if (!this.person) return;
    this.linkingFamily = true;
    this.linkFamilyError = '';

    this.familyService.linkFamilyToPerson(data.familyId, this.person.id, {
      relationship: data.relationship,
      isPrimary: data.isPrimary
    }).subscribe({
      next: () => {
        this.linkingFamily = false;
        this.toastService.success('Familiar vinculado exitosamente');
        this.showLinkModal = false;
        this.loadRepresentatives();
      },
      error: (err) => {
        this.linkingFamily = false;
        this.linkFamilyError = err?.userMessage || 'Error al vincular el familiar';
      },
    });
  }

  confirmUnlinkFromChild(observation: string): void {
    if (!this.person || !this.unlinkingRepresentative || !observation.trim()) return;
    this.unlinking = true;

    this.familyService.unlinkFamilyFromPerson(
      this.unlinkingRepresentative.representativeId,
      this.person.id,
      observation
    ).subscribe({
      next: () => {
        this.unlinking = false;
        this.toastService.success('Familiar desvinculado exitosamente');
        this.showUnlinkModal = false;
        this.loadRepresentatives();
      },
      error: () => {
        this.unlinking = false;
        this.toastService.error('Error al desvincular el familiar');
      },
    });
  }

  openHistoryModal(): void {
    if (!this.person) return;
    this.linkHistory = [];
    this.loadingHistory = true;
    this.showHistoryModal = true;

    this.familyService.getPersonLinkHistory(this.person.id).subscribe({
      next: (data) => {
        this.linkHistory = data ?? [];
        this.loadingHistory = false;
      },
      error: () => {
        this.loadingHistory = false;
        this.toastService.error('Error al cargar historial');
      },
    });
  }

  closeHistoryModal(): void {
    this.showHistoryModal = false;
  }
}
