import { Directive, EventEmitter, Input, OnDestroy, Output, ViewContainerRef } from '@angular/core';
import { AppTemplateService } from './app-template.service';
import { Subscription } from 'rxjs/Subscription';
import 'rxjs/add/operator/delay';

@Directive({
  selector: '[appRenderTemplate]'
})
export class ToolbarTemplateDirective implements OnDestroy {
  subscription: Subscription;
  hasView = false;
  _templateName: string;

  @Input('appRenderTemplate')
  set templateName(templateName: string) {
    if (this.subscription) this.subscription.unsubscribe();
    this._templateName = templateName;
    this.subscription = this.toolbarService.ObserveTemplateRef(templateName || 'toolbar')
      .delay(0)
      .subscribe(templateRef => {
        let originalValue = this.hasView;
        try {
          if (this.hasView) {
            this.viewContainer.clear();
            this.hasView = false;
          }
          if (!templateRef) return;
          this.viewContainer.createEmbeddedView(templateRef);
          this.hasView = true;
        } finally {
          if (originalValue != this.hasView) this.onHasView.emit(this.hasView);
        }
      });
  }

  get templateName() {
    return this._templateName;
  }

  @Output() onHasView = new EventEmitter<boolean>();

  constructor(private viewContainer: ViewContainerRef,
              private toolbarService: AppTemplateService) {

  }


  ngOnDestroy() {
    if (this.subscription) this.subscription.unsubscribe();
  }


}
