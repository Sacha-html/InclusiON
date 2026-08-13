import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AppRoutes } from '@shared/constants/app-routes';
import { ProfessionalsService, AssignmentsService, ActivitiesService, FamilyService, ToastService } from '@services';
import { MessagesService } from '@services/messages.service';
import {
  ProfessionalPersonResponse,
  ActivityAssignmentResponse,
  ActivityAttemptResponse,
  ActivityAssignmentStatus,
  ActivityResponseResult,
  PersonRepresentativeResponse
} from '@models';
import { switchMap } from 'rxjs';
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
  FormControlDirective
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';
import { ActorAvatarComponent } from '@shared/components/actor-avatar/actor-avatar.component';

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
    IconDirective,
    ActorAvatarComponent
  ],
  templateUrl: './evaluations.component.html',
  styleUrl: './evaluations.component.scss'
})
export class EvaluationsComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly professionalsService = inject(ProfessionalsService);
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly activitiesService = inject(ActivitiesService);
  private readonly familyService = inject(FamilyService);
  private readonly messagesService = inject(MessagesService);
  private readonly toastService = inject(ToastService);

  persons = signal<ProfessionalPersonResponse[]>([]);
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

  // Modal State
  showShareModal = signal<boolean>(false);
  representativesList = signal<PersonRepresentativeResponse[]>([]);
  selectedTutorId = '';
  shareMessageBody = '';
  sendingShare = signal<boolean>(false);

  ngOnInit(): void {
    this.loadPersons();
  }

  loadPersons(): void {
    this.isLoadingPersons.set(true);
    this.professionalsService.getMyProfile().pipe(
      switchMap(prof => this.assignmentsService.getPersonsByProfessional(prof.id))
    ).subscribe({
      next: (data) => {
        this.persons.set(data.filter(p => p.isActive));
        this.isLoadingPersons.set(false);
      },
      error: () => {
        this.isLoadingPersons.set(false);
        this.toastService.error('Error al cargar la lista de alumnos');
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
      },
      error: () => {
        this.isLoadingAssignments.set(false);
        this.toastService.error('Error al cargar las evaluaciones del alumno');
      }
    });
  }

  calculateMetrics(data: ActivityAssignmentResponse[]): void {
    let completed = 0;
    let inProgress = 0;
    let pending = 0;
    let totalSuccess = 0;
    let totalTime = 0;
    let responseCount = 0;

    data.forEach(a => {
      if (a.status === 'Completada') completed++;
      else if (a.status === 'EnProgreso') inProgress++;
      else pending++;

      if (a.responses) {
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
    });

    this.completedCount.set(completed);
    this.inProgressCount.set(inProgress);
    this.pendingCount.set(pending);
    this.totalAttempts.set(responseCount);
    this.averageSuccessRate.set(responseCount > 0 ? (totalSuccess / responseCount) : 0);
    this.averageTimeSpent.set(responseCount > 0 ? (totalTime / responseCount) : 0);
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
      default: return 'info';
    }
  }

  getAssignmentStatusLabel(status: ActivityAssignmentStatus | string): string {
    switch (status) {
      case 'Completada': return 'Completada';
      case 'EnProgreso': return 'En progreso';
      case 'Pendiente': return 'Pendiente';
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
