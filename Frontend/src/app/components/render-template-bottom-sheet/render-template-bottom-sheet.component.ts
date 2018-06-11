import { Component, Inject, OnInit, TemplateRef } from '@angular/core';
import { MAT_BOTTOM_SHEET_DATA, MatBottomSheet, MatBottomSheetRef } from '@angular/material';
import { AppTemplateService } from '../../toolbar/app-template.service';
import { Observable } from 'rxjs/Rx';

@Component({
  selector: 'app-render-template-bottom-sheet',
  templateUrl: './render-template-bottom-sheet.component.html',
  styleUrls: ['./render-template-bottom-sheet.component.scss']
})
export class RenderTemplateBottomSheetComponent implements OnInit {
  templateObservable: Observable<TemplateRef<any>>;
  context: any;

  constructor(@Inject(MAT_BOTTOM_SHEET_DATA) data: { template: string, context: any },
              public sheetRef: MatBottomSheetRef<RenderTemplateBottomSheetComponent>,
              appTemplateService: AppTemplateService) {
    this.templateObservable = appTemplateService.ObserveTemplateRef(data.template);
    this.context = {$implicit: sheetRef, ...data.context};
  }

  static Open(bottomSheet: MatBottomSheet, template: string, context: any = {}) {
    return bottomSheet.open(RenderTemplateBottomSheetComponent, {
      data: {
        template: template,
        context: context
      }
    });
  }

  static OpenWait(bottomSheet: MatBottomSheet, template: string) {
    return this.Open(bottomSheet, template).afterDismissed().toPromise();
  }

  ngOnInit() {
  }

}
