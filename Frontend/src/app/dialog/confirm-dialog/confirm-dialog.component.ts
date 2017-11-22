import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';

@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.component.html',
  styleUrls: ['./confirm-dialog.component.scss']
})
export class ConfirmDialogComponent {

  constructor(@Inject(MAT_DIALOG_DATA) public data: any, public dialogRef: MatDialogRef<ConfirmDialogComponent>) {
  }

  static Options(title: string, acceptText: string, rejectText: string) {
    return {title: title, acceptText: acceptText, rejectText: rejectText};
  }

  reject() {
    this.dialogRef.close();
  }
}
