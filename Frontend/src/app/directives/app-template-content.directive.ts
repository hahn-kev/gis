import { Directive, Input, OnDestroy, TemplateRef } from '@angular/core';
import { AppTemplateService } from '../toolbar/app-template.service';

@Directive({
  selector: '[appTemplateContent]'
})
export class AppTemplateContentDirective implements OnDestroy {
  constructor(private templateRef: TemplateRef<any>, private toolbarService: AppTemplateService) {

  }

  _templateName: string;
  @Input('appTemplateContent') set templateName(value: string) {
    this._templateName = value;
    this.toolbarService.setTemplate(value, this.templateRef);
  }

  ngOnDestroy() {
    this.toolbarService.setTemplate(this._templateName, null);
  }


}
