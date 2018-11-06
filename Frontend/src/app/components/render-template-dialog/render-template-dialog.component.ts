import {Component, Inject, OnInit, TemplateRef} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialog, MatDialogRef} from '@angular/material';
import {Observable} from 'rxjs';
import {AppTemplateService} from '../../toolbar/app-template.service';

interface RenderDialogContext {
  $implicit: MatDialogRef<RenderTemplateDialogComponent>;
}

@Component({
  selector: 'app-render-template-dialog',
  templateUrl: './render-template-dialog.component.html',
  styleUrls: ['./render-template-dialog.component.scss']
})
export class RenderTemplateDialogComponent implements OnInit {
  templateObservable: Observable<TemplateRef<RenderDialogContext>>;
  template: TemplateRef<RenderDialogContext>;

  constructor(@Inject(MAT_DIALOG_DATA) public data: { title: string, template: string | TemplateRef<RenderDialogContext> },
              public dialogRef: MatDialogRef<RenderTemplateDialogComponent>,
              appTemplateService: AppTemplateService) {
    if (typeof data.template == 'string') {
      this.templateObservable = appTemplateService.ObserveTemplateRef('dialog:' + data.template);
    } else {
      this.template = data.template;
    }
  }

  static Open(dialog: MatDialog, title: string, template: TemplateRef<RenderDialogContext>) {
    return dialog.open(RenderTemplateDialogComponent, {
      data: {
        title: title,
        template: template
      }
    });
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
