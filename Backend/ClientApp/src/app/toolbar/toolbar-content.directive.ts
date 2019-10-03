import { Directive, OnDestroy, TemplateRef } from '@angular/core';
import { AppTemplateService } from './app-template.service';

@Directive({
  selector: '[appToolbarContent]'
})
export class ToolbarContentDirective implements OnDestroy {
  constructor(private templateRef: TemplateRef<any>, private toolbarService: AppTemplateService) {

    this.toolbarService.setTemplate('toolbar', this.templateRef);
  }

  ngOnDestroy() {
    this.toolbarService.setTemplate('toolbar', null);
  }
}
