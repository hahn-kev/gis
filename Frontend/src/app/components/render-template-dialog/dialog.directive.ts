import { Directive, Input, TemplateRef } from '@angular/core';
import { MatDialog } from '@angular/material';
import { RenderTemplateDialogComponent } from './render-template-dialog.component';

@Directive({
  selector: '[appDialog]',
  exportAs: 'dialog'
})
export class DialogDirective {
  @Input('appDialog') title: string;

  constructor(private templateRef: TemplateRef<any>, private dialog: MatDialog) {

  }

  show() {
    return RenderTemplateDialogComponent.Open(this.dialog, this.title, this.templateRef);
  }

}
