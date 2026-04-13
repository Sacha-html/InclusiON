import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ReportsService } from '@services';
import { CreateReportRequest } from '@models/requests/reports/create-report.request';
import { PersonsService } from '@services';
import { PersonListItemResponse } from '@models/responses/person.response';
import {
  CardComponent,
  CardBodyComponent,
  CardHeaderComponent,
  ButtonDirective,
  ColComponent,
  RowComponent,
  FormControlDirective,
  SpinnerComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-report-new',
  standalone: true,
  imports: [
    FormsModule,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    ButtonDirective,
    ColComponent,
    RowComponent,
    FormControlDirective,
    SpinnerComponent,
  ],
  templateUrl: './new.component.html',
  styleUrl: './new.component.scss',
})
export class NewComponent implements OnInit {
  private readonly reportsService = inject(ReportsService);
  private readonly personsService = inject(PersonsService);
  private readonly router = inject(Router);

  persons = signal<PersonListItemResponse[]>([]);
  filteredPersons = signal<PersonListItemResponse[]>([]);
  personSearch = signal('');
  isLoading = signal(false);

  form = signal<CreateReportRequest>({
    personId: '',
    title: '',
    content: '',
    reportTypeId: 0,
    reportDate: new Date().toISOString().split('T')[0],
    periodStartDate: '',
    periodEndDate: '',
    achievedGoals: '',
    areasToReinforce: '',
    futureRecommendations: '',
    nextObjectives: '',
  });

  isValid = computed(() => {
    const f = this.form();
    return f.personId !== '' && f.title.trim() !== '' && f.content.trim() !== '' && f.reportTypeId > 0;
  });

  ngOnInit(): void {
    this.loadPersons();
  }

  loadPersons(): void {
    this.personsService.getPersons({ pageSize: 100 }).subscribe({
      next: (response) => {
        this.persons.set(response.data);
        this.filteredPersons.set(response.data);
      },
    });
  }

  onPersonSearch(term: string): void {
    this.personSearch.set(term);
    if (!term.trim()) {
      this.filteredPersons.set(this.persons());
    } else {
      const lower = term.toLowerCase();
      this.filteredPersons.set(
        this.persons().filter(p => 
          `${p.firstName} ${p.lastName}`.toLowerCase().includes(lower)
        )
      );
    }
  }

  onSubmit(): void {
    this.isLoading.set(true);
    this.reportsService.create(this.form()).subscribe({
      next: () => {
        this.router.navigate(['/pro/reports']);
      },
      error: () => this.isLoading.set(false),
    });
  }

  onCancel(): void {
    this.router.navigate(['/pro/reports']);
  }
}