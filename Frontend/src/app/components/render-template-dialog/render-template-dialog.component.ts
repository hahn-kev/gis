import { Component, Inject, OnInit, TemplateRef } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material';
import { Observable } from 'rxjs/Rx';
import { AppTemplateService } from '../../toolbar/app-template.service';

@Component({
  selector: 'app-render-template-dialog',
  templateUrl: './render-template-dialog.component.html',
  styleUrls: ['./render-template-dialog.component.scss']
})
export class RenderTemplateDialogComponent implements OnInit {
  templateObservable: Observable<TemplateRef<{ $implicit: MatDialogRef<RenderTemplateDialogComponent> }>>;

  constructor(@Inject(MAT_DIALOG_DATA) public data: { title: string, template: string },
              public dialogRef: MatDialogRef<RenderTemplateDialogComponent>,
              appTemplateService: AppTemplateService) {
    this.templateObservable = appTemplateService.ObserveTemplateRef('dialog:' + data.template);
  }

  static OpenWait(dialog: MatDialog, title: string, template: string) {
    return dialog.open(RenderTemplateDialogComponent, {
      data: {
        title: title,
        template: template
      }
    }).afterClosed().toPromise();
  }

  ngOnInit() {
  }

}
