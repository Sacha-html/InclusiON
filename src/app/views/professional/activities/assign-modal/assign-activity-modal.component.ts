import { Component, EventEmitter, Input, OnChanges, Output, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { switchMap } from 'rxjs';
import { ActivitiesService } from '@services/activities.service';
import { AssignmentsService } from '@services/assignments.service';
import { ProfessionalsService } from '@services/professionals.service';
import { ToastService } from '@services';
import { ActivityListItemResponse } from '@models/responses/activity.response';
import { ProfessionalPersonResponse } from '@models';
import {
  ModalComponent, ModalHeaderComponent, ModalBodyComponent, ModalFooterComponent,
  ButtonDirective, SpinnerComponent,
  FormSelectDirective, FormControlDirective, FormCheckComponent,
  FormCheckInputDirective, FormCheckLabelDirective,
  ColComponent, RowComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-assign-activity-modal',
  standalone: true,
  imports: [
    FormsModule,
    ModalComponent, ModalHeaderComponent, ModalBodyComponent, ModalFooterComponent,
    ButtonDirective, SpinnerComponent,
    FormSelectDirective, FormControlDirective,
    FormCheckComponent, FormCheckInputDirective, FormCheckLabelDirective,
    ColComponent, RowComponent,
  ],
  templateUrl: './assign-activity-modal.component.html',
})
export class AssignActivityModalComponent implements OnChanges {
  @Input() visible = false;
  @Input() activity: ActivityListItemResponse | null = null;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() assigned      = new EventEmitter<void>();

  private readonly activitiesService    = inject(ActivitiesService);
  private readonly assignmentsService   = inject(AssignmentsService);
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly toastService         = inject(ToastService);

  persons      = signal<ProfessionalPersonResponse[]>([]);
  isLoadingPersons = signal(false);
  isSaving     = signal(false);

  form = {
    personId:            '',
    dueDate:             '',
    isEvaluationActivity: false,
  };

  get activePersons(): ProfessionalPersonResponse[] {
    return this.persons().filter(p => p.isActive);
  }

  get isValid(): boolean {
    return !!this.form.personId;
  }

  get today(): string {
    return new Date().toISOString().split('T')[0];
  }

  ngOnChanges(): void {
    if (this.visible && this.persons().length === 0) {
      this.loadPersons();
    }
    if (this.visible) {
      this.form = { personId: '', dueDate: '', isEvaluationActivity: false };
    }
  }

  private loadPersons(): void {
    this.isLoadingPersons.set(true);
    this.professionalsService.getMyProfile().pipe(
      switchMap(prof => this.assignmentsService.getPersonsByProfessional(prof.id))
    ).subscribe({
      next:  (persons) => { this.persons.set(persons); this.isLoadingPersons.set(false); },
      error: ()        => { this.isLoadingPersons.set(false); this.toastService.error('Error al cargar personas'); },
    });
  }

  save(): void {
    if (!this.isValid || !this.activity) return;
    this.isSaving.set(true);

    this.activitiesService.createAssignment({
      activityId:           this.activity.id,
      personId:             this.form.personId,
      dueDate:              this.form.dueDate || undefined,
      isEvaluationActivity: this.form.isEvaluationActivity,
    }).subscribe({
      next: () => {
        this.toastService.success('Actividad asignada exitosamente.');
        this.isSaving.set(false);
        this.close();
        this.assigned.emit();
      },
      error: () => {
        this.toastService.error('Error al asignar la actividad.');
        this.isSaving.set(false);
      },
    });
  }

  close(): void {
    this.visibleChange.emit(false);
  }
}
