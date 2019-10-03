import { Pipe, PipeTransform } from '@angular/core';
import { AccordionListFormDirective } from './accordion-list-form.directive';
import { BaseEntity } from '../../classes/base-entity';

@Pipe({
  name: 'findForm'
})
export class FindFormPipe implements PipeTransform {

  transform(forms: AccordionListFormDirective<BaseEntity>[], index: number): AccordionListFormDirective<BaseEntity> {
    for (let form of forms) {
      if (form.getContext().index == index) return form;
    }
    return null;
  }

}
