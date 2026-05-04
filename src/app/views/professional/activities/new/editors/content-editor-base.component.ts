import { Directive, EventEmitter, Input, Output } from '@angular/core';

@Directive()
export abstract class ContentEditorBaseComponent {
  @Input() initialJson: string = '{}';
  @Output() contentChange = new EventEmitter<string>();
  @Output() validChange   = new EventEmitter<boolean>();
}
