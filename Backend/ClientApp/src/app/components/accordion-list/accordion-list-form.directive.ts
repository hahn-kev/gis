import { Directive, ElementRef, Host, OnDestroy } from '@angular/core';
import { NgForm } from '@angular/forms';
import { AccordionListComponent } from './accordion-list.component';
import { BaseEntity } from '../../classes/base-entity';

@Directive({
  selector: '[appAccordionListForm]'
})
export class AccordionListFormDirective<T extends BaseEntity> implements OnDestroy {
  getContext(): { $implicit: T; index: number } {
    let index = this.host.findIndex(this);
    return {$implicit: index == -1 ? this.host.newItem : this.host.items[index], index: index};
  }

  constructor(@Host() public ngForm: NgForm,
              @Host() public host: AccordionListComponent<T>,
              private element: ElementRef<HTMLElement>) {
    this.ngForm.ngSubmit.subscribe(() => {
      let context = this.getContext();
      return this.host.onSave(context.$implicit,
        this.host.expansionPanels.toArray()[context.index + 1],
        this.ngForm, context.index == -1);
    });
    setTimeout(() => {
      this.host.addForm(this);
    });
  }

  hasAsParent(parent: HTMLElement) {
    let currentElement = this.element.nativeElement;
    while (currentElement != null && !this.host.matchesElement(currentElement)) {
      if (currentElement == parent) return true;
      currentElement = currentElement.parentElement;
    }
    return false;
  }

  ngOnDestroy(): void {
    this.host.removeForm(this);
  }

}
