import { Directive, Input, OnDestroy, TemplateRef, ViewContainerRef } from '@angular/core';
import { Subscription } from 'rxjs';
import { LoginService } from '../services/auth/login.service';

@Directive({
  selector: '[requireRole]'
})
export class RequireRoleDirective implements OnDestroy {
  private hasView = false;
  private hasRoleSubscription: Subscription;

  constructor(private templateRef: TemplateRef<any>,
              private viewContainer: ViewContainerRef,
              private loginService: LoginService) {
  }

  @Input() set requireRole(roles: string) {
    if (this.hasRoleSubscription) this.hasRoleSubscription.unsubscribe();
    this.hasRoleSubscription = this.loginService.hasAnyRole(roles.split(','))
      .subscribe(hasRole => this.updateView(hasRole));
  }

  updateView(hasRole: boolean) {
    if (hasRole && !this.hasView) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.hasView = true;
    } else if (!hasRole && this.hasView) {
      this.viewContainer.clear();
      this.hasView = false;
    }
  }

  ngOnDestroy() {
    this.hasRoleSubscription.unsubscribe();
  }
}
