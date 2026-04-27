import { Component, ElementRef, EventEmitter, Input, OnChanges, Output, SimpleChanges, ViewChild } from '@angular/core';
import {
  ButtonDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
} from '@coreui/angular';

@Component({
  selector: 'app-password-modal',
  standalone: true,
  imports: [
    ModalComponent, ModalHeaderComponent, ModalBodyComponent, ModalFooterComponent,
    ButtonDirective,
  ],
  templateUrl: './password-modal.component.html',
})
export class PasswordModalComponent implements OnChanges {
  @Input() visible = false;
  @Input() entityType = 'Usuario';
  @Input() entityArticle = 'el';
  @Input() entityName = '';
  @Input() password = '';
  @Output() close = new EventEmitter<void>();

  @ViewChild('closeBtn', { read: ElementRef }) closeBtn?: ElementRef<HTMLButtonElement>;

  copied = false;

  get entityTypeLower(): string { return this.entityType.toLowerCase(); }
  get entityArticleUpper(): string { return this.entityArticle.charAt(0).toUpperCase() + this.entityArticle.slice(1); }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['visible']?.currentValue === true) {
      setTimeout(() => this.closeBtn?.nativeElement.focus(), 150);
    }
  }

  copyPassword(): void {
    navigator.clipboard.writeText(this.password).then(() => {
      this.copied = true;
      setTimeout(() => this.copied = false, 2000);
    });
  }

  onVisibleChange(visible: boolean): void {
    if (!visible) {
      this.close.emit();
    }
  }
}
