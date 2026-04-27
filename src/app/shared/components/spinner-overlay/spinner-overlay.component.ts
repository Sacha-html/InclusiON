import { Component, inject } from '@angular/core';
import { SpinnerComponent } from '@coreui/angular';
import { SpinnerService } from '@services';

@Component({
  selector: 'app-spinner-overlay',
  standalone: true,
  imports: [SpinnerComponent],
  templateUrl: './spinner-overlay.component.html',
  styleUrl: './spinner-overlay.component.css'
})
export class SpinnerOverlayComponent {
  readonly spinnerService = inject(SpinnerService);
}