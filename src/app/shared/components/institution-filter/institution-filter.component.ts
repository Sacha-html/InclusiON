import { Component, EventEmitter, inject, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormSelectDirective } from '@coreui/angular';
import { AdminInstitutionsService } from '@services';
import { AdminInstitutionResponse } from '@models';

@Component({
  selector: 'app-institution-filter',
  standalone: true,
  imports: [FormsModule, FormSelectDirective],
  template: `
    @if (!isGlobalAdmin && institutions.length > 1) {
      <div class="mb-3">
        <label for="institutionFilter" class="form-label">Filtrar por institucion</label>
        <select cSelect id="institutionFilter" [(ngModel)]="selectedId" (ngModelChange)="onFilterChange()">
          @for (inst of institutions; track inst.institutionId) {
            <option [ngValue]="inst.institutionId">{{ inst.institutionName }}</option>
          }
        </select>
      </div>
    }
  `,
})
export class InstitutionFilterComponent implements OnInit {
  private readonly adminInstitutionsService = inject(AdminInstitutionsService);

  institutions: AdminInstitutionResponse[] = [];
  selectedId: number | undefined;
  isGlobalAdmin = true;

  @Output() filterChange = new EventEmitter<number | undefined>();
  @Output() loaded = new EventEmitter<void>();

  ngOnInit(): void {
    this.adminInstitutionsService.getMyInstitutions().subscribe({
      next: (institutions) => {
        this.institutions = institutions;
        if (institutions.length > 0) {
          this.isGlobalAdmin = false;
          this.selectedId = institutions[0].institutionId;
        }
        this.filterChange.emit(this.selectedId);
        this.loaded.emit();
      },
      error: () => {
        this.filterChange.emit(undefined);
        this.loaded.emit();
      },
    });
  }

  onFilterChange(): void {
    this.filterChange.emit(this.selectedId);
  }
}
