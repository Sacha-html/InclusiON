import { Component, computed, inject, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AppRoutes } from '@shared/constants/app-routes';
import { ProfessionalsService, AssignmentsService, ActivitiesService, FamilyService, ToastService } from '@services';
import { MessagesService } from '@services/messages.service';
import {
  ProfessionalPersonResponse,
  ClassroomResponse,
  ActivityAssignmentResponse,
  ActivityAttemptResponse,
  ActivityAssignmentStatus,
  ActivityResponseResult,
  PersonRepresentativeResponse
} from '@models';
import { forkJoin, Subscription, switchMap } from 'rxjs';
import {
  CardComponent,
  CardBodyComponent,
  CardHeaderComponent,
  ColComponent,
  RowComponent,
  SpinnerComponent,
  BadgeComponent,
  TableDirective,
  ButtonDirective,
  ProgressComponent,
  ProgressBarComponent,
  ModalComponent,
  ModalHeaderComponent,
  ModalBodyComponent,
  ModalFooterComponent,
  ModalTitleDirective,
  FormSelectDirective,
  FormControlDirective,
  AlertComponent
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';
import { ActorAvatarComponent } from '@shared/components/actor-avatar/actor-avatar.component';
import { TimeFormatPipe } from '@shared/pipes';

@Component({
  selector: 'app-evaluations',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    DecimalPipe,
    FormsModule,
    CardComponent,
    CardBodyComponent,
    CardHeaderComponent,
    ColComponent,
    RowComponent,
    SpinnerComponent,
    BadgeComponent,
    TableDirective,
    ButtonDirective,
    ProgressComponent,
    ProgressBarComponent,
    ModalComponent,
    ModalHeaderComponent,
    ModalBodyComponent,
    ModalFooterComponent,
    ModalTitleDirective,
    FormSelectDirective,
    FormControlDirective,
    AlertComponent,
    IconDirective,
    ActorAvatarComponent,
    TimeFormatPipe,
  ],
  templateUrl: './evaluations.component.html',
  styleUrl: './evaluations.component.scss'
})
export class EvaluationsComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly activitiesService = inject(ActivitiesService);
  private readonly familyService = inject(FamilyService);
  private readonly messagesService = inject(MessagesService);
  private readonly toastService = inject(ToastService);

  private routeSub?: Subscription;

  // Query Params & Notification Navigation
  targetPersonId = signal<string | null>(null);
  targetActivityId = signal<number | null>(null);
  showAlertFromNotification = signal<boolean>(false);
  dismissedAlertAssignmentIds = signal<Set<number>>(new Set());

  persons = signal<ProfessionalPersonResponse[]>([]);
  classrooms = signal<ClassroomResponse[]>([]);
  selectedClassroomId = signal<string>('');
  searchStudentQuery = signal<string>('');

  selectedPerson = signal<ProfessionalPersonResponse | null>(null);
  assignments = signal<ActivityAssignmentResponse[]>([]);
  
  isLoadingPersons = signal<boolean>(true);
  isLoadingAssignments = signal<boolean>(false);

  // Expanded attempts mapping
  expandedAssignments = signal<Set<number>>(new Set());

  // Computed metrics
  completedCount = signal<number>(0);
  inProgressCount = signal<number>(0);
  pendingCount = signal<number>(0);
  averageSuccessRate = signal<number>(0);
  averageTimeSpent = signal<number>(0);
  totalAttempts = signal<number>(0);

  // Alertas de Frustración / Estancamiento
  hasFrustrationAlerts = signal<boolean>(false);
  frustrationAlertCount = signal<number>(0);
  frustratedActivities = signal<string[]>([]);

  // Modal State - Share with tutor
  showShareModal = signal<boolean>(false);
  representativesList = signal<PersonRepresentativeResponse[]>([]);
  selectedTutorId = '';
  shareMessageBody = '';
  sendingShare = signal<boolean>(false);

  // Modal State - Modificar / Adaptación Pedagógica
  showAdaptationModal = signal<boolean>(false);
  selectedAssignmentForAdaptation = signal<ActivityAssignmentResponse | null>(null);
  adaptationDueDate = signal<string>('');
  adaptationEstimatedDuration = signal<number | null>(null);
  adaptationHasVisualSupport = signal<boolean>(false);
  adaptationHasAudioSupport = signal<boolean>(false);
  adaptationUsesEasyReading = signal<boolean>(false);
  adaptationUsesPictograms = signal<boolean>(false);
  adaptationRequiresSupervision = signal<boolean>(false);
  adaptationNotes = signal<string>('');
  adaptationAcknowledgeAlert = signal<boolean>(true);
  isSavingAdaptation = signal<boolean>(false);

  // Modal State - Dar de baja (Opción A)
  showDeactivateModal = signal<boolean>(false);
  assignmentToDeactivate = signal<ActivityAssignmentResponse | null>(null);
  isDeactivating = signal<boolean>(false);

  // Computed signal to filter students by Classroom and search term
  filteredPersons = computed(() => {
    const classroomId = this.selectedClassroomId();
    const search = this.searchStudentQuery().toLowerCase().trim();
    let list = this.persons();

    if (classroomId) {
      const selectedRoom = this.classrooms().find(
        c => c.id === classroomId || (c.id && c.id.toLowerCase() === classroomId.toLowerCase())
      );
      const targetName = selectedRoom?.name?.toLowerCase()?.trim();
      const targetIdRaw = classroomId.replace(/^ENC:/i, '').toLowerCase().trim();

      list = list.filter(p => {
        if (targetName && p.classroomName?.toLowerCase()?.trim() === targetName) {
          return true;
        }
        if (p.classroomId) {
          const personRoomIdRaw = p.classroomId.replace(/^ENC:/i, '').toLowerCase().trim();
          if (personRoomIdRaw === targetIdRaw) {
            return true;
          }
        }
        return false;
      });
    }

    if (search) {
      list = list.filter(p => 
        p.personFullName.toLowerCase().includes(search) ||
        (p.personLastName && p.personLastName.toLowerCase().includes(search)) ||
        (p.personFirstName && p.personFirstName.toLowerCase().includes(search)) ||
        (p.personDocumentNumber && p.personDocumentNumber.includes(search))
      );
    }

    return list;
  });

  ngOnInit(): void {
    this.routeSub = this.route.queryParams.subscribe(params => {
      const pId = params['personId'];
      const aId = params['activityId'];
      const alertType = params['alert'];

      if (pId) {
        this.targetPersonId.set(String(pId));
      }
      if (aId) {
        this.targetActivityId.set(Number(aId));
      }
      if (alertType) {
        this.showAlertFromNotification.set(true);
      }

      this.tryAutoSelectPerson();
    });

    this.loadPersons();
  }

  ngOnDestroy(): void {
    this.routeSub?.unsubscribe();
  }

  private tryAutoSelectPerson(): void {
    const targetId = this.targetPersonId();
    if (!targetId || this.persons().length === 0) return;

    const targetIdRaw = targetId.replace(/^ENC:/i, '').toLowerCase().trim();
    const found = this.persons().find(p => {
      const pIdStr = String(p.personId);
      const pIdRaw = pIdStr.replace(/^ENC:/i, '').toLowerCase().trim();
      return pIdStr === targetId || pIdRaw === targetIdRaw;
    });

    if (found && this.selectedPerson()?.personId !== found.personId) {
      this.selectedClassroomId.set('');
      this.selectPerson(found);
    }
  }

  loadPersons(): void {
    this.isLoadingPersons.set(true);
    this.professionalsService.getMyProfile().pipe(
      switchMap(prof => forkJoin({
        persons: this.assignmentsService.getPersonsByProfessional(prof.id),
        classrooms: this.assignmentsService.getClassroomsByProfessional(prof.id)
      }))
    ).subscribe({
      next: ({ persons, classrooms }) => {
        this.persons.set(persons.filter(p => p.isActive));
        this.classrooms.set(classrooms);
        this.isLoadingPersons.set(false);
        this.tryAutoSelectPerson();
      },
      error: () => {
        this.isLoadingPersons.set(false);
        this.toastService.error('Error al cargar la lista de alumnos y aulas');
      }
    });
  }

  selectPerson(person: ProfessionalPersonResponse): void {
    this.selectedPerson.set(person);
    this.expandedAssignments.set(new Set());
    this.loadAssignments(person.personId);
  }

  loadAssignments(personId: string): void {
    this.isLoadingAssignments.set(true);
    this.activitiesService.getPersonAssignments(personId).subscribe({
      next: (data) => {
        this.assignments.set(data);
        this.calculateMetrics(data);
        this.isLoadingAssignments.set(false);

        const targetActId = this.targetActivityId();
        if (targetActId) {
          const match = data.find(a => a.activityId === targetActId || a.id === targetActId);
          if (match) {
            this.expandedAssignments.update(s => new Set(s).add(match.id));
          }
        }
      },
      error: () => {
        this.isLoadingAssignments.set(false);
        this.toastService.error('Error al cargar las evaluaciones del alumno');
      }
    });
  }

  isActivityStruggling(a: ActivityAssignmentResponse): boolean {
    if (a.status === 'Completada' || a.status === 'Cancelada') return false;
    if (this.dismissedAlertAssignmentIds().has(a.id)) return false;

    if (!a.responses || a.responses.length === 0) return false;

    let relevantResponses = a.responses;
    if (a.alertAcknowledgedAt) {
      const ackDate = new Date(a.alertAcknowledgedAt).getTime();
      relevantResponses = a.responses.filter(r => {
        if (!r.startedAt) return false;
        return new Date(r.startedAt).getTime() > ackDate;
      });
    }

    if (relevantResponses.length === 0) return false;

    const failedAttempts = relevantResponses.filter(r => 
      r.result === 'Fallido' || 
      (r.successPercentage !== null && r.successPercentage !== undefined && Number(r.successPercentage) < 50)
    );
    const hasFrustrationAttempt = relevantResponses.some(r => 
      (r.frustrationLevel !== undefined && r.frustrationLevel > 0) || 
      (r.successPercentage !== null && r.successPercentage !== undefined && Number(r.successPercentage) <= 40)
    );

    return failedAttempts.length >= 2 || relevantResponses.length >= 4 || hasFrustrationAttempt;
  }

  isTargetActivity(a: ActivityAssignmentResponse): boolean {
    const targetActId = this.targetActivityId();
    return targetActId !== null && (a.activityId === targetActId || a.id === targetActId);
  }

  calculateMetrics(data: ActivityAssignmentResponse[]): void {
    let completed = 0;
    let inProgress = 0;
    let pending = 0;
    let totalSuccess = 0;
    let totalTime = 0;
    let responseCount = 0;
    let frustrationCount = 0;
    const frustratedActivityNames: string[] = [];

    data.forEach(a => {
      if (a.status === 'Completada') completed++;
      else if (a.status === 'EnProgreso') inProgress++;
      else if (a.status === 'Pendiente') pending++;

      if (a.responses && a.responses.length > 0) {
        a.responses.forEach(r => {
          responseCount++;
          if (r.successPercentage !== undefined && r.successPercentage !== null) {
            totalSuccess += Number(r.successPercentage);
          }
          if (r.timeSpentSeconds) {
            totalTime += r.timeSpentSeconds;
          }
        });
      }

      if (this.isActivityStruggling(a)) {
        frustrationCount++;
        frustratedActivityNames.push(a.activityTitle);
      }
    });

    this.completedCount.set(completed);
    this.inProgressCount.set(inProgress);
    this.pendingCount.set(pending);
    this.totalAttempts.set(responseCount);
    this.averageSuccessRate.set(responseCount > 0 ? (totalSuccess / responseCount) : 0);
    this.averageTimeSpent.set(responseCount > 0 ? (totalTime / responseCount) : 0);

    this.frustrationAlertCount.set(frustrationCount);
    this.hasFrustrationAlerts.set(frustrationCount > 0);
    this.frustratedActivities.set(frustratedActivityNames);
  }

  dismissAlertBanner(): void {
    this.showAlertFromNotification.set(false);
    const struggling = this.assignments().filter(a => this.isActivityStruggling(a));
    struggling.forEach(a => {
      this.dismissedAlertAssignmentIds.update(set => new Set(set).add(a.id));
      this.activitiesService.updateAssignmentAdaptation(a.id, {
        acknowledgeAlert: true
      }).subscribe();
    });

    const targetActId = this.targetActivityId();
    if (targetActId) {
      const match = this.assignments().find(a => a.activityId === targetActId || a.id === targetActId);
      if (match && !this.dismissedAlertAssignmentIds().has(match.id)) {
        this.dismissedAlertAssignmentIds.update(set => new Set(set).add(match.id));
        this.activitiesService.updateAssignmentAdaptation(match.id, {
          acknowledgeAlert: true
        }).subscribe();
      }
    }

    this.calculateMetrics(this.assignments());
    this.toastService.success('Alerta marcada como atendida hasta nuevo aviso.');
  }

  // ── Modal Modificar / Adaptación Pedagógica ────────────────────────────
  openAdaptationModal(assignment: ActivityAssignmentResponse, event?: Event): void {
    if (event) event.stopPropagation();
    this.selectedAssignmentForAdaptation.set(assignment);
    this.adaptationDueDate.set(assignment.dueDate ? assignment.dueDate.split('T')[0] : '');
    this.adaptationEstimatedDuration.set(assignment.estimatedDurationMinutes ?? null);
    this.adaptationHasVisualSupport.set(assignment.hasVisualSupport ?? false);
    this.adaptationHasAudioSupport.set(assignment.hasAudioSupport ?? false);
    this.adaptationUsesEasyReading.set(assignment.usesEasyReading ?? false);
    this.adaptationUsesPictograms.set(assignment.usesPictograms ?? false);
    this.adaptationRequiresSupervision.set(assignment.requiresSupervision ?? false);
    this.adaptationNotes.set(assignment.customAdaptationNotes || '');
    this.adaptationAcknowledgeAlert.set(true);
    this.showAdaptationModal.set(true);
  }

  closeAdaptationModal(): void {
    this.showAdaptationModal.set(false);
    this.selectedAssignmentForAdaptation.set(null);
  }

  saveAdaptation(): void {
    const assignment = this.selectedAssignmentForAdaptation();
    if (!assignment) return;

    this.isSavingAdaptation.set(true);
    const dueDateVal = this.adaptationDueDate() ? new Date(this.adaptationDueDate() + 'T23:59:59').toISOString() : null;

    this.activitiesService.updateAssignmentAdaptation(assignment.id, {
      dueDate: dueDateVal,
      estimatedDurationMinutes: this.adaptationEstimatedDuration(),
      hasVisualSupport: this.adaptationHasVisualSupport(),
      hasAudioSupport: this.adaptationHasAudioSupport(),
      usesEasyReading: this.adaptationUsesEasyReading(),
      usesPictograms: this.adaptationUsesPictograms(),
      requiresSupervision: this.adaptationRequiresSupervision(),
      customAdaptationNotes: this.adaptationNotes().trim() || null,
      acknowledgeAlert: this.adaptationAcknowledgeAlert()
    }).subscribe({
      next: (updated) => {
        this.isSavingAdaptation.set(false);
        this.toastService.success('Actividad modificada y adaptada exitosamente.');
        this.closeAdaptationModal();

        this.assignments.update(list => list.map(a => a.id === assignment.id ? {
          ...a,
          dueDate: updated.dueDate !== undefined ? updated.dueDate : a.dueDate,
          estimatedDurationMinutes: updated.estimatedDurationMinutes !== undefined ? updated.estimatedDurationMinutes : this.adaptationEstimatedDuration() ?? a.estimatedDurationMinutes,
          hasVisualSupport: updated.hasVisualSupport !== undefined ? updated.hasVisualSupport : this.adaptationHasVisualSupport(),
          hasAudioSupport: updated.hasAudioSupport !== undefined ? updated.hasAudioSupport : this.adaptationHasAudioSupport(),
          usesEasyReading: updated.usesEasyReading !== undefined ? updated.usesEasyReading : this.adaptationUsesEasyReading(),
          usesPictograms: updated.usesPictograms !== undefined ? updated.usesPictograms : this.adaptationUsesPictograms(),
          requiresSupervision: updated.requiresSupervision !== undefined ? updated.requiresSupervision : this.adaptationRequiresSupervision(),
          customAdaptationNotes: updated.customAdaptationNotes !== undefined ? updated.customAdaptationNotes : this.adaptationNotes().trim(),
          alertAcknowledgedAt: updated.alertAcknowledgedAt !== undefined ? updated.alertAcknowledgedAt : new Date().toISOString()
        } : a));

        if (this.adaptationAcknowledgeAlert()) {
          this.dismissedAlertAssignmentIds.update(s => new Set(s).add(assignment.id));
          if (assignment.activityId === this.targetActivityId() || assignment.id === this.targetActivityId()) {
            this.showAlertFromNotification.set(false);
          }
        }
        this.calculateMetrics(this.assignments());
      },
      error: () => {
        this.isSavingAdaptation.set(false);
        this.toastService.error('Error al guardar las modificaciones de la actividad.');
      }
    });
  }

  // ── Modal Dar de baja actividad (Opción A) ─────────────────────────────
  openDeactivateModal(assignment: ActivityAssignmentResponse, event?: Event): void {
    if (event) event.stopPropagation();
    this.assignmentToDeactivate.set(assignment);
    this.showDeactivateModal.set(true);
  }

  closeDeactivateModal(): void {
    this.showDeactivateModal.set(false);
    this.assignmentToDeactivate.set(null);
  }

  confirmDeactivate(): void {
    const assignment = this.assignmentToDeactivate();
    if (!assignment) return;

    this.isDeactivating.set(true);
    this.activitiesService.cancelAssignment(assignment.id).subscribe({
      next: () => {
        this.isDeactivating.set(false);
        this.toastService.success(`La actividad "${assignment.activityTitle}" fue dada de baja.`);
        this.dismissedAlertAssignmentIds.update(s => new Set(s).add(assignment.id));
        if (assignment.activityId === this.targetActivityId() || assignment.id === this.targetActivityId()) {
          this.showAlertFromNotification.set(false);
        }
        this.closeDeactivateModal();

        this.assignments.update(list => list.map(a => a.id === assignment.id ? { ...a, status: ActivityAssignmentStatus.Cancelada } : a));
        this.calculateMetrics(this.assignments());
      },
      error: () => {
        this.isDeactivating.set(false);
        this.toastService.error('Error al dar de baja la actividad.');
      }
    });
  }

  formatTime(seconds: number | undefined): string {
    if (!seconds) return '—';
    if (seconds < 60) return `${seconds}s`;
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return secs > 0 ? `${mins}m ${secs}s` : `${mins}m`;
  }

  getMaxSuccess(responses: ActivityAttemptResponse[] | undefined): number {
    if (!responses || responses.length === 0) return 0;
    return Math.max(...responses.map(r => r.successPercentage !== null && r.successPercentage !== undefined ? Number(r.successPercentage) : 0));
  }

  isExpanded(assignmentId: number): boolean {
    return this.expandedAssignments().has(assignmentId);
  }

  toggleExpanded(assignmentId: number): void {
    const current = new Set(this.expandedAssignments());
    if (current.has(assignmentId)) {
      current.delete(assignmentId);
    } else {
      current.add(assignmentId);
    }
    this.expandedAssignments.set(current);
  }

  getResultBadgeColor(result: ActivityResponseResult | string | undefined): string {
    if (!result) return 'secondary';
    switch (result) {
      case 'Exito':
      case ActivityResponseResult.Exito:
        return 'success';
      case 'Parcial':
      case ActivityResponseResult.Parcial:
        return 'warning';
      case 'Fallido':
      case ActivityResponseResult.Fallido:
        return 'danger';
      default:
        return 'secondary';
    }
  }

  getResultLabel(result: ActivityResponseResult | string | undefined): string {
    if (!result) return 'Pendiente';
    switch (result) {
      case 'Exito':
      case ActivityResponseResult.Exito:
        return 'Éxito';
      case 'Parcial':
      case ActivityResponseResult.Parcial:
        return 'Parcial';
      case 'Fallido':
      case ActivityResponseResult.Fallido:
        return 'Fallido';
      default:
        return result.toString();
    }
  }

  getAssignmentStatusColor(status: ActivityAssignmentStatus | string): string {
    switch (status) {
      case 'Completada': return 'success';
      case 'EnProgreso': return 'warning';
      case 'Pendiente': return 'secondary';
      case 'Cancelada': return 'danger';
      default: return 'info';
    }
  }

  getAssignmentStatusLabel(status: ActivityAssignmentStatus | string): string {
    switch (status) {
      case 'Completada': return 'Completada';
      case 'EnProgreso': return 'En progreso';
      case 'Pendiente': return 'Pendiente';
      case 'Cancelada': return 'Cancelada';
      default: return status.toString();
    }
  }

  getFrustrationEmoji(level: number | undefined): string {
    if (!level) return '—';
    if (level <= 1) return '😊 (Muy bajo)';
    if (level === 2) return '🙂 (Bajo)';
    if (level === 3) return '😐 (Moderado)';
    if (level === 4) return '🙁 (Alto)';
    return '😫 (Muy alto)';
  }

  // ── Download Student Metrics PDF ───────────────────────────────────────
  downloadStudentMetricsPdf(): void {
    const student = this.selectedPerson();
    if (!student) return;

    const printWindow = window.open('', '_blank');
    if (!printWindow) {
      this.toastService.error('Por favor, permite ventanas emergentes para descargar el PDF.');
      return;
    }

    const assignmentsHtml = this.assignments().map(a => `
      <tr>
        <td style="padding: 12px 10px; border-bottom: 1px solid #eee; font-size: 13px;">
          <strong>${a.activityTitle}</strong><br>
          <span style="font-size: 11px; color: #777;">Código: ${a.templateTypeCode}</span>
        </td>
        <td style="padding: 12px 10px; border-bottom: 1px solid #eee; font-size: 13px;">${this.getAssignmentStatusLabel(a.status)}</td>
        <td style="padding: 12px 10px; border-bottom: 1px solid #eee; font-size: 13px;">${new Date(a.assignedAt).toLocaleDateString('es-ES')}</td>
        <td style="padding: 12px 10px; border-bottom: 1px solid #eee; font-size: 13px; text-align: center;">${a.responses?.length || 0}</td>
        <td style="padding: 12px 10px; border-bottom: 1px solid #eee; font-size: 13px; text-align: center; font-weight: bold; color: #2e7d32;">
          ${a.responses && a.responses.length > 0 ? this.getMaxSuccess(a.responses) + '%' : '—'}
        </td>
      </tr>
    `).join('');

    printWindow.document.write(`
      <html>
        <head>
          <title>Reporte de Métricas - ${student.personFullName}</title>
          <style>
            body { font-family: 'Helvetica Neue', Arial, sans-serif; padding: 40px; color: #333; line-height: 1.5; }
            .header { border-bottom: 2px solid #0096c7; padding-bottom: 20px; margin-bottom: 30px; }
            .logo { font-size: 24px; font-weight: bold; color: #0077b6; }
            .student-info { font-size: 20px; margin-top: 10px; font-weight: 600; color: #111; }
            .dni { font-size: 13px; color: #666; margin-top: 5px; }
            .grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 15px; margin-bottom: 35px; }
            .card { border: 1px solid #e0e0e0; padding: 18px; border-radius: 10px; background: #fcfcfc; box-shadow: 0 2px 4px rgba(0,0,0,0.01); }
            .label { font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px; color: #777; font-weight: 600; }
            .value { font-size: 24px; font-weight: bold; color: #0077b6; margin-top: 5px; }
            .table-container { margin-top: 30px; }
            h3 { font-size: 16px; font-weight: 600; border-left: 4px solid #0077b6; padding-left: 10px; margin-bottom: 15px; }
            table { width: 100%; border-collapse: collapse; margin-top: 10px; }
            th { background-color: #f8f9fa; padding: 12px 10px; text-align: left; font-size: 11px; text-transform: uppercase; color: #666; font-weight: 600; border-bottom: 2px solid #dee2e6; }
            .footer { border-top: 1px solid #eee; padding-top: 20px; font-size: 11px; color: #999; margin-top: 60px; text-align: center; }
          </style>
        </head>
        <body>
          <div class="header">
            <div class="logo">InclusiON</div>
            <div class="student-info">Métricas de Desempeño Escolar: ${student.personFullName}</div>
            <div class="dni">DNI / Nro. Documento: ${student.personDocumentNumber || '—'}</div>
          </div>
          <div class="grid">
            <div class="card">
              <div class="label">Tasa de Acierto</div>
              <div class="value">${Math.round(this.averageSuccessRate())}%</div>
            </div>
            <div class="card">
              <div class="label">Tiempo Promedio</div>
              <div class="value">${this.formatTime(this.averageTimeSpent())}</div>
            </div>
            <div class="card">
              <div class="label">Completadas</div>
              <div class="value">${this.completedCount()}</div>
            </div>
            <div class="card">
              <div class="label">Total Intentos</div>
              <div class="value">${this.totalAttempts()}</div>
            </div>
          </div>
          <div class="table-container">
            <h3>Historial de Avance de Actividades</h3>
            <table>
              <thead>
                <tr>
                  <th>Actividad</th>
                  <th>Estado</th>
                  <th>Asignado el</th>
                  <th style="text-align: center;">Intentos</th>
                  <th style="text-align: center;">Mejor Acierto</th>
                </tr>
              </thead>
              <tbody>
                ${assignmentsHtml || '<tr><td colspan="5" style="text-align: center; padding: 20px; color: #999;">Sin actividades asignadas</td></tr>'}
              </tbody>
            </table>
          </div>
          <div class="footer">
            Generado automáticamente por el portal profesional de InclusiON el ${new Date().toLocaleDateString('es-ES')} a las ${new Date().toLocaleTimeString('es-ES')}.
          </div>
          <script>
            window.onload = function() {
              window.print();
              setTimeout(function() { window.close(); }, 500);
            };
          </script>
        </body>
      </html>
    `);
    printWindow.document.close();
    this.toastService.success('Preparando PDF de métricas...');
  }

  // ── Share Student Metrics with Tutor ───────────────────────────────────
  openShareModal(): void {
    const student = this.selectedPerson();
    if (!student) return;

    this.selectedTutorId = '';
    const successRate = Math.round(this.averageSuccessRate());
    const avgTime = this.formatTime(this.averageTimeSpent());

    this.shareMessageBody = `Estimado Tutor, le comparto las métricas de rendimiento y avance de ${student.personFullName}:\n\n` +
      `- Tasa de acierto promedio: ${successRate}%\n` +
      `- Tiempo promedio por intento: ${avgTime}\n` +
      `- Actividades completadas: ${this.completedCount()} (en curso: ${this.inProgressCount()}, pendientes: ${this.pendingCount()})\n` +
      `- Cantidad total de intentos: ${this.totalAttempts()}\n\n` +
      `Quedo a su entera disposición para analizar en conjunto la evolución del alumno.`;

    this.familyService.getPersonRepresentatives(student.personId).subscribe({
      next: (list) => {
        this.representativesList.set(list);
        this.showShareModal.set(true);
      },
      error: () => {
        this.toastService.error('Error al obtener la lista de tutores del alumno.');
      }
    });
  }

  closeShareModal(): void {
    this.showShareModal.set(false);
  }

  sendSharedMetrics(): void {
    const tutorId = this.selectedTutorId;
    if (!tutorId) {
      this.toastService.error('Por favor, selecciona un tutor destinatario.');
      return;
    }
    if (!this.shareMessageBody.trim()) {
      this.toastService.error('El contenido del mensaje no puede estar vacío.');
      return;
    }

    this.sendingShare.set(true);
    this.messagesService.send({
      receiverId: tutorId,
      subject: `Métricas de Avance - ${this.selectedPerson()?.personFullName}`,
      content: this.shareMessageBody.trim()
    }).subscribe({
      next: () => {
        this.sendingShare.set(false);
        this.showShareModal.set(false);
        this.toastService.success('Métricas compartidas por mensajería exitosamente.');
      },
      error: () => {
        this.sendingShare.set(false);
        this.toastService.error('Error al enviar las métricas.');
      }
    });
  }

  navigateToNewActivity(): void {
    this.router.navigate([AppRoutes.Pro.ActivityNew]);
  }
}
