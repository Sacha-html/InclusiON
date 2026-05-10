import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PersonsService, ToastService } from '@services';
import { DiagnosesService } from '@services/diagnoses.service';
import {
  PersonResponse,
  PersonSkillProfileResponse,
  DiagnosisListItemResponse,
} from '@models';
import {
  BadgeComponent,
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
} from '@coreui/angular';
import { ProfessionalPersonDataComponent } from './components/professional-person-data.component';
import { ProfessionalFunctionalProfileComponent } from './components/professional-functional-profile.component';
import { ProfessionalSkillsComponent } from './components/professional-skills.component';
import { ProfessionalDiagnosesComponent } from './components/professional-diagnoses.component';
import { FamilyService } from '@services';
import { PersonRepresentativeResponse } from '@models';
import { ProfessionalFamilyTabComponent } from './components/professional-family-tab.component';
import { ProfessionalActivitiesTabComponent } from './components/professional-activities-tab.component';
import { ProfessionalRoadmapTabComponent } from './components/professional-roadmap-tab.component';
import { AppRoutes } from '@shared/constants/app-routes';

@Component({
  selector: 'app-person-detail',
  standalone: true,
  imports: [
    BadgeComponent,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    ButtonDirective,
    ProfessionalPersonDataComponent,
    ProfessionalFunctionalProfileComponent,
    ProfessionalSkillsComponent,
    ProfessionalDiagnosesComponent,
    ProfessionalFamilyTabComponent,
    ProfessionalActivitiesTabComponent,
    ProfessionalRoadmapTabComponent,
  ],
  templateUrl: './person-detail.component.html',
  styleUrl: './person-detail.component.scss',
})
export class PersonDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly personsService = inject(PersonsService);
  private readonly familyService = inject(FamilyService);
  private readonly toastService = inject(ToastService);
  private readonly diagnosesService = inject(DiagnosesService);

  person: PersonResponse | null = null;
  activeTab: 'datos' | 'funcional' | 'habilidades' | 'diagnosticos' | 'familiares' | 'actividades' | 'roadmap' = 'datos';

  skillProfile = signal<PersonSkillProfileResponse[]>([]);
  diagnoses = signal<DiagnosisListItemResponse[]>([]);

  representatives: PersonRepresentativeResponse[] = [];
  loadingRepresentatives = false;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.personsService.getPersonById(id).subscribe({
        next: (person) => {
          this.person = person;
          this.loadSkillProfile();
          this.loadDiagnoses();
        },
        error: () => this.router.navigate([AppRoutes.Pro.Persons]),
      });
    }
  }

  private loadSkillProfile(): void {
    if (!this.person) return;
    this.personsService.getSkillProfile(this.person.id).subscribe({
      next: (data) => this.skillProfile.set(data ?? []),
      error: () => this.toastService.error('Error al cargar el perfil de habilidades'),
    });
  }

  private loadDiagnoses(): void {
    if (!this.person) return;
    this.diagnosesService.getByPerson(this.person.id).subscribe({
      next: (data) => this.diagnoses.set(data),
      error: () => this.toastService.error('Error al cargar los diagnósticos'),
    });
  }

  goBack(): void {
    this.router.navigate([AppRoutes.Pro.Persons]);
  }

  onPersonChange(person: PersonResponse): void {
    this.person = person;
  }

  onSkillProfileChange(data: PersonSkillProfileResponse[]): void {
    this.skillProfile.set(data);
  }

  onDiagnosesChange(data: DiagnosisListItemResponse[]): void {
    this.diagnoses.set(data);
  }

  loadRepresentatives(): void {
    if (!this.person) return;
    this.loadingRepresentatives = true;
    this.familyService.getPersonRepresentatives(this.person.id).subscribe({
      next: (data) => {
        this.representatives = data;
        this.loadingRepresentatives = false;
      },
      error: () => {
        this.loadingRepresentatives = false;
        this.toastService.error('Error al cargar los familiares');
      },
    });
  }

  onRefreshFamily(): void {
    this.loadRepresentatives();
  }
}


