import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ActionItem, TableColumn } from './data-table.models';
import {
  ButtonDirective,
  CardBodyComponent,
  CardComponent,
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

  getVisibleActions(col: TableColumn, item: any): ActionItem[] {
    if (!col.actions) return [];
    return col.actions.filter(a => !a.visible || a.visible(item));
  }
}
