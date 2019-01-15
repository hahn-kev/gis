import { Directive, EmbeddedViewRef, Host, TemplateRef } from '@angular/core';
import { AccordionListComponent } from './accordion-list.component';
import { BaseEntity } from '../../classes/base-entity';

@Directive({
  selector: '[appAccordionListContent]'
})
export class AccordionListContentDirective<T extends BaseEntity> extends TemplateRef<{ $implicit: T, index: number }> {

  constructor(@Host() parent: AccordionListComponent<T>,
              private templateRef: TemplateRef<{ $implicit: T, index: number }>) {
    super();
    parent.content = this;
  }

  createEmbeddedView(context: { $implicit: T; index: number }): EmbeddedViewRef<{ $implicit: T; index: number }> {
    return this.templateRef.createEmbeddedView(context);
  }

  get elementRef() {
    return this.templateRef.elementRef;
  }

}
