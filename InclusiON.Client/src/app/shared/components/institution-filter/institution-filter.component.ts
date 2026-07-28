import { Component, EventEmitter, inject, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormSelectDirective } from '@coreui/angular';
import { AdminInstitutionsService } from '@services';
import { AdminInstitutionResponse } from '@models';

@Component({
  selector: 'app-institution-filter',
  standalone: true,
  imports: [FormsModule, FormSelectDirective],
  templateUrl: './institution-filter.component.html',
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
