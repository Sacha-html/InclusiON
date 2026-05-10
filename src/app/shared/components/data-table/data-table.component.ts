import { Component, EventEmitter, Input, Output, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { ActiveStatus } from '@shared/constants/status-labels';
import { Subject, Subscription, debounceTime, distinctUntilChanged } from 'rxjs';
import { ActionItem, HeaderButton, TableColumn } from './data-table.models';
import {
  BadgeComponent,
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  CardHeaderComponent,
  ColComponent,
  DropdownComponent,
  DropdownItemDirective,
  DropdownMenuDirective,
  DropdownToggleDirective,
  FormControlDirective,
  InputGroupComponent,
  InputGroupTextDirective,
  PageItemComponent,
  PageLinkDirective,
  PaginationComponent,
  RowComponent,
  TableDirective,
  SpinnerComponent,
} from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';

@Component({
  selector: 'app-data-table',
  imports: [
    NgTemplateOutlet,
    BadgeComponent,
    TableDirective,
    CardComponent,
    CardHeaderComponent,
    CardBodyComponent,
    PaginationComponent,
    PageItemComponent,
    PageLinkDirective,
    ButtonDirective,
    InputGroupComponent,
    InputGroupTextDirective,
    FormControlDirective,
    RowComponent,
    ColComponent,
    DropdownComponent,
    DropdownToggleDirective,
    DropdownMenuDirective,
    DropdownItemDirective,
    IconDirective,
    SpinnerComponent,
  ],
  templateUrl: './data-table.component.html',
  styleUrl: './data-table.component.scss',
})
export class DataTableComponent implements OnInit, OnDestroy {
  @Input() title: string = '';
  @Input() showTitle: boolean = true;
  @Input() columns: TableColumn[] = [];
  @Input() items: any[] = [];
  @Input() totalItems: number = 0;
  @Input() pageSize: number = 10;
  @Input() currentPage: number = 1;
  @Input() headerButtons: HeaderButton[] = [];
  @Input() showSearch: boolean = true;
  @Input() showPagination: boolean = true;
  @Input() debounceMs: number = 400;
  @Input() sortable: boolean = false;
  @Input() loading: boolean = false;
  @Input() emptyMessage = 'Sin registros';
  @Input() emptyIcon    = '';
  @Input() emptyDetail  = '';
  @Input() showCard = true;

  @Output() pageChange = new EventEmitter<number>();
  @Output() searchAction = new EventEmitter<string>();
  @Output() sortAction = new EventEmitter<{ sortBy: string; sortDirection: 'ASC' | 'DESC' }>();
  @Output() rowAction = new EventEmitter<{ action: string; item: any }>();
  @Output() headerAction = new EventEmitter<string>();

  sortField = '';
  sortDirection: 'ASC' | 'DESC' = 'ASC';

  private searchSubject = new Subject<string>();
  private searchSub!: Subscription;

  ngOnInit(): void {
    this.searchSub = this.searchSubject.pipe(
      debounceTime(this.debounceMs),
      distinctUntilChanged(),
    ).subscribe(term => this.searchAction.emit(term));
  }

  ngOnDestroy(): void {
    this.searchSub?.unsubscribe();
  }

  onSearchInput(value: string): void {
    this.searchSubject.next(value);
  }

  get totalPages(): number {
    return Math.ceil(this.totalItems / this.pageSize);
  }

  get pages(): (number | '...')[] {
    const total = this.totalPages;
    const current = this.currentPage;
    const maxVisible = 5;

    if (total <= maxVisible) {
      return Array.from({ length: total }, (_, i) => i + 1);
    }

    const pages: (number | '...')[] = [];
    pages.push(1);

    let start = Math.max(2, current - 1);
    let end = Math.min(total - 1, current + 1);

    if (current <= 3) {
      end = Math.min(total - 1, maxVisible - 1);
    }
    if (current >= total - 2) {
      start = Math.max(2, total - maxVisible + 2);
    }

    if (start > 2) pages.push('...');
    for (let i = start; i <= end; i++) pages.push(i);
    if (end < total - 1) pages.push('...');

    pages.push(total);
    return pages;
  }

  get showingFrom(): number {
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  get showingTo(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalItems);
  }

  getVisibleActions(col: TableColumn, item: any): ActionItem[] {
    if (!col.actions) return [];
    return col.actions.filter(a => !a.visible || a.visible(item));
  }

  onSort(col: TableColumn): void {
    if (!col.sortable) return;
    if (this.sortField === col.key) {
      this.sortDirection = this.sortDirection === 'ASC' ? 'DESC' : 'ASC';
    } else {
      this.sortField = col.key;
      this.sortDirection = 'ASC';
    }
    this.sortAction.emit({ sortBy: this.sortField, sortDirection: this.sortDirection });
  }

  getAriaSort(col: TableColumn): 'ascending' | 'descending' | 'none' | null {
    if (!col.sortable) return null;
    if (this.sortField !== col.key) return 'none';
    return this.sortDirection === 'ASC' ? 'ascending' : 'descending';
  }

  getBadgeColor(value: any, col?: TableColumn): string {
    if (col?.badgeMap) {
      const key = String(value);
      return col.badgeMap[key]?.color || 'secondary';
    }
    if (typeof value === 'boolean') return value ? 'success' : 'danger';
    switch (value?.toLowerCase()) {
      case 'approved': return 'success';
      case 'terminated': return 'secondary';
      case 'suspended': return 'warning';
      case 'rejected': return 'danger';
      case 'admin': return 'primary';
      case 'professional': return 'info';
      case 'familyrepresentative': return 'success';
      case 'personwithdisability': return 'warning';
      default: return 'info';
    }
  }

  getBadgeLabel(value: any, col?: TableColumn): string {
    if (col?.badgeMap) {
      const key = String(value);
      return col.badgeMap[key]?.label || '-';
    }
    if (typeof value === 'boolean') return value ? ActiveStatus.Activo : ActiveStatus.Inactivo;
    switch (value?.toLowerCase()) {
      case 'approved': return 'Aprobado';
      case 'terminated': return 'Dado de baja';
      case 'suspended': return 'Suspendido';
      case 'rejected': return 'Rechazado';
      case 'admin': return 'Administrador';
      case 'professional': return 'Profesional';
      case 'familyrepresentative': return 'Familiar';
      case 'personwithdisability': return 'Persona';
      default: return value ?? '';
    }
  }

  formatDate(val: any): string {
    if (!val) return '-';
    try {
      return new Date(val).toLocaleDateString('es-AR', {
        day: '2-digit', month: '2-digit', year: 'numeric',
      });
    } catch {
      return String(val);
    }
  }
}
