import { Directive, TemplateRef, ViewContainerRef, inject, OnInit } from '@angular/core';
import { AuthService } from '@services';

@Directive({
  selector: '[appIfGlobalAdmin]',
  standalone: true,
})
export class IfGlobalAdminDirective implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly templateRef = inject(TemplateRef<any>);
  private readonly viewContainer = inject(ViewContainerRef);

  private hasView = false;

  ngOnInit(): void {
    const isGlobal = this.authService.isGlobalAdmin();

    if (isGlobal && !this.hasView) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.hasView = true;
    } else if (!isGlobal && this.hasView) {
      this.viewContainer.clear();
      this.hasView = false;
    }
  }
}
