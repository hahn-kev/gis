import { Directive, TemplateRef } from '@angular/core';
import { MatBottomSheet } from '@angular/material';

@Directive({
  selector: '[appBottomSheet]',
  exportAs: 'bottomSheet'
})
export class BottomSheetDirective {
  constructor(private templateRef: TemplateRef<any>, private bottomSheet: MatBottomSheet) {

  }

  show(context?: any) {
    return this.bottomSheet.open(this.templateRef, {
      data: context
    });
  }

  dismiss() {
    this.bottomSheet.dismiss();
  }

}
