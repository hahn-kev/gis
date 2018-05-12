import { Directive, Host } from '@angular/core';
import { NgForm } from '@angular/forms';
import { AccordionListComponent } from './accordion-list.component';

@Directive({
  selector: '[appAccordionListForm]'
})
export class AccordionListFormDirective<T> {
  public context: { $implicit: T; index: number };

  constructor(@Host() public ngForm: NgForm,
              @Host() public host: AccordionListComponent<T>) {
    this.context = this.host.content.lastCreatedTemplateContext;
    setTimeout(() => {
      if (this.context.index == -1) this.host.newForm = this;
      else this.host.forms[this.context.index] = this;
    });
  }

}
