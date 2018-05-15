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
  templateObservable: Observable<TemplateRef<{ $implicit: MatBottomSheetRef<RenderTemplateBottomSheetComponent> }>>;

  constructor(@Inject(MAT_BOTTOM_SHEET_DATA) data: { template: string },
              public sheetRef: MatBottomSheetRef<RenderTemplateBottomSheetComponent>,
              appTemplateService: AppTemplateService) {
    this.templateObservable = appTemplateService.ObserveTemplateRef(data.template);
  }

  static Open(bottomSheet: MatBottomSheet, template: string) {
    return bottomSheet.open(RenderTemplateBottomSheetComponent, {
      data: {
        template: template
      }
    });
  }

  static OpenWait(bottomSheet: MatBottomSheet, template: string) {
    return this.Open(bottomSheet, template).afterDismissed().toPromise();
  }

  ngOnInit() {
  }

}
