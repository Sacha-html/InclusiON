import { Component, EventEmitter, Input, Output, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { Subject, Subscription, debounceTime, distinctUntilChanged } from 'rxjs';
import { ActionItem, HeaderButton, TableColumn } from './data-table.models';
import {
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
} from '@coreui/angular';

@Component({
  selector: 'app-data-table',
  imports: [
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
  ],
  templateUrl: './data-table.component.html',
  styleUrl: './data-table.component.scss',
})
export class DataTableComponent implements OnInit, OnDestroy {
  @Input() title: string = '';
  @Input() columns: TableColumn[] = [];
  @Input() items: any[] = [];
  @Input() totalItems: number = 0;
  @Input() pageSize: number = 10;
  @Input() currentPage: number = 1;
  @Input() headerButtons: HeaderButton[] = [];
  @Input() showSearch: boolean = true;
  @Input() debounceMs: number = 400;

  @Output() pageChange = new EventEmitter<number>();
  @Output() searchAction = new EventEmitter<string>();
  @Output() rowAction = new EventEmitter<{ action: string; item: any }>();
  @Output() headerAction = new EventEmitter<string>();

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

  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  getVisibleActions(col: TableColumn, item: any): ActionItem[] {
    if (!col.actions) return [];
    return col.actions.filter(a => !a.visible || a.visible(item));
  }
}
