import { Directive, Input } from '@angular/core';


@Directive({
  selector: '[appTemplateContent]'
})
// tslint:disable-next-line:component-class-suffix
export class MockAppTemplateContentDirective {
  @Input('appTemplateContent') appTemplateContent: string;
}
