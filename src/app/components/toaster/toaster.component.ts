import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { ToastModule } from '@coreui/angular';
import { ToastService } from '../../services/toast.service';
import { Toast } from '../../models';

@Component({
  selector: 'app-toaster',
  imports: [CommonModule, ToastModule],
  templateUrl: './toaster.component.html',
  styleUrl: './toaster.component.scss',
})
export class ToasterComponent implements OnInit {
  private readonly toastSvc = inject(ToastService);
  toasts: Toast[] = [];

  ngOnInit(): void {
    this.toastSvc.toasts$.subscribe((p) => {
      this.toasts.push(p);
    });
  }

  onVisibleChange(visible: boolean, toast: Toast) {
    if (!visible) {
      this.toasts = this.toasts.filter((t) => t.id !== toast.id);
    }
  }
}
