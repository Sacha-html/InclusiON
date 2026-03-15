import { Component, EventEmitter, Input, Output } from '@angular/core';
import { TableColumn } from './data-table.models';
import {
  BadgeComponent,
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
  ColComponent,
  FormControlDirective,
  InputGroupComponent,
  InputGroupTextDirective,
  PageItemComponent,
  PageLinkDirective,
  PaginationComponent,
  RowComponent,
  TableDirective,
} from '@coreui/angular';

@Component({
  selector: 'app-data-table',
  imports: [
    TableDirective,
    CardComponent,
    PaginationComponent,
    CardBodyComponent,
    PageItemComponent,
    PageLinkDirective,
    BadgeComponent,
    ButtonDirective,
    InputGroupComponent,
    InputGroupTextDirective,
    FormControlDirective,
    RowComponent,
    ColComponent,
  ],
  templateUrl: './data-table.component.html',
  styleUrl: './data-table.component.scss',
})
export class DataTableComponent {
  @Input() columns: TableColumn[] = [];
  @Input() items: any[] = [];
  @Input() totalItems: number = 0;
  @Input() pageSize: number = 10;
  @Input() currentPage: number = 1;

  @Output() pageChange = new EventEmitter<number>();
  @Output() searchAction = new EventEmitter<string>();
  @Output() rowAction = new EventEmitter<{ action: string; item: any }>();

  get totalPages(): number {
    return Math.ceil(this.totalItems / this.pageSize);
  }

  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }
}
