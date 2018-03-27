import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material';

@Component({
  selector: 'app-render-template-dialog',
  templateUrl: './render-template-dialog.component.html',
  styleUrls: ['./render-template-dialog.component.scss']
})
export class RenderTemplateDialogComponent implements OnInit {
  static OpenWait(dialog: MatDialog, title: string, template: string) {
    return dialog.open(RenderTemplateDialogComponent, {
      data: {
        title: title,
        template: template
      }
    }).afterClosed().toPromise();
  }

  constructor(@Inject(MAT_DIALOG_DATA) public data: { title: string, template: string },
              public dialogRef: MatDialogRef<RenderTemplateDialogComponent>) {
  }

  ngOnInit() {
  }

}
