import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { PersonsService, ToastService } from '@services';
import {
  PersonResponse,
  PersonSkillProfileResponse,
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
import { ProfessionalFamilyTabComponent } from './components/family-tab.component';

@Component({
  selector: 'app-person-detail',
  standalone: true,
  imports: [
    CommonModule,
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

  person: PersonResponse | null = null;
  activeTab: 'datos' | 'funcional' | 'habilidades' | 'diagnosticos' | 'familiares' = 'datos';

  skillProfile = signal<PersonSkillProfileResponse[]>([]);
  diagnoses = signal<any[]>([]);

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
        error: () => this.router.navigate(['/pro/persons']),
      });
    }
  }

  private loadSkillProfile(): void {
    if (!this.person) return;
    this.personsService.getSkillProfile(this.person.id).subscribe({
      next: (data) => this.skillProfile.set(data ?? []),
    });
  }

  private loadDiagnoses(): void {}

  goBack(): void {
    this.router.navigate(['/pro/persons']);
  }

  onPersonChange(person: PersonResponse): void {
    this.person = person;
  }

  onSkillProfileChange(data: PersonSkillProfileResponse[]): void {
    this.skillProfile.set(data);
  }

  onDiagnosesChange(data: any[]): void {
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
      },
    });
  }

  onRefreshFamily(): void {
    this.loadRepresentatives();
  }
}


