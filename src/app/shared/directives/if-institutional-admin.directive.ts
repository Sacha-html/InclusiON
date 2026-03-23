import { Directive, TemplateRef, ViewContainerRef, inject, OnInit } from '@angular/core';
import { AuthService } from '@services';

@Directive({
  selector: '[appIfInstitutionalAdmin]',
  standalone: true,
})
export class IfInstitutionalAdminDirective implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly templateRef = inject(TemplateRef<any>);
  private readonly viewContainer = inject(ViewContainerRef);

  private hasView = false;

  ngOnInit(): void {
    const isInstitutional = this.authService.getUserRole() === 'Admin' && !this.authService.isGlobalAdmin();

    if (isInstitutional && !this.hasView) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.hasView = true;
    } else if (!isInstitutional && this.hasView) {
      this.viewContainer.clear();
      this.hasView = false;
    }
  }
}
