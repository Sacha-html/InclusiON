import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { ToastModule } from '@coreui/angular';
import { Subscription } from 'rxjs';
import { ToastService } from '@services';
import { Toast } from '@models';

@Component({
  selector: 'app-toaster',
  imports: [CommonModule, ToastModule],
  templateUrl: './toaster.component.html',
  styleUrl: './toaster.component.scss',
})
export class ToasterComponent implements OnInit, OnDestroy {
  private readonly toastSvc = inject(ToastService);
  private subscription: Subscription | null = null;

  toasts: Toast[] = [];

  ngOnInit(): void {
    this.subscription = this.toastSvc.toasts$.subscribe((toast) => {
      this.toasts.push(toast);
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  onVisibleChange(visible: boolean, toast: Toast): void {
    if (!visible) {
      this.toasts = this.toasts.filter((t) => t.id !== toast.id);
    }
  }
}
