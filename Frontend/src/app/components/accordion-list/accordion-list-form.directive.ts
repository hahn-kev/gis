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
    this.ngForm.ngSubmit.subscribe(() => this.host.onSave(this.context.$implicit,
      this.host.expansionPanels.toArray()[this.context.index + 1],
      this.ngForm, this.context.index == -1));
    setTimeout(() => {
      if (this.context.index == -1) this.host.newForm = this;
      else this.host.forms[this.context.index] = this;
    });
  }

}
