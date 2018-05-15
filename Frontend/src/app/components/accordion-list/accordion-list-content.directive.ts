import { Directive, EmbeddedViewRef, Host, TemplateRef } from '@angular/core';
import { AccordionListComponent } from './accordion-list.component';

@Directive({
  selector: '[appAccordionListContent]'
})
export class AccordionListContentDirective<T> extends TemplateRef<{ $implicit: T, index: number }> {
  lastCreatedTemplateContext: { $implicit: T; index: number };

  constructor(@Host() parent: AccordionListComponent<T>,
              private templateRef: TemplateRef<{ $implicit: T, index: number }>) {
    super();
    parent.content = this;
  }

  createEmbeddedView(context: { $implicit: T; index: number }): EmbeddedViewRef<{ $implicit: T; index: number }> {
    this.lastCreatedTemplateContext = context;
    let ref = this.templateRef.createEmbeddedView(context);
    return ref;
  }

  get elementRef() {
    return this.templateRef.elementRef;
  }

}
