import { Component, inject, OnInit, OnDestroy, HostListener, ViewChild, ElementRef } from '@angular/core';
import { ToastModule } from '@coreui/angular';
import { IconDirective } from '@coreui/icons-angular';
import { Subscription } from 'rxjs';
import { ToastService } from '@services';
import { Toast } from '@models';

@Component({
  selector: 'app-toaster',
  imports: [ToastModule, IconDirective],
  templateUrl: './toaster.component.html',
  styleUrl: './toaster.component.scss',
})
export class ToasterComponent implements OnInit, OnDestroy {
  private readonly toastSvc = inject(ToastService);
  private subscription: Subscription | null = null;

  @ViewChild('toasterRegion') toasterRegion!: ElementRef<HTMLElement>;

  toasts: Toast[] = [];

  // WCAG 2.1.1 — permite descartar el toast más reciente con Escape
  @HostListener('document:keydown.escape')
  onEscapeKey(): void {
    if (this.toasts.length > 0) {
      const last = this.toasts[this.toasts.length - 1];
      this.toasts = this.toasts.filter(t => t.id !== last.id);
    }
  }

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

  getIcon(color: string): string {
    switch (color) {
      case 'success': return 'cil-check-circle';
      case 'danger': return 'cil-warning';
      case 'warning': return 'cil-warning';
      case 'info': return 'cil-info';
      default: return 'cil-info';
    }
  }
}
