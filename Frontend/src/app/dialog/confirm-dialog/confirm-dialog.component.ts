import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material';

@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.component.html',
  styleUrls: ['./confirm-dialog.component.scss']
})
export class ConfirmDialogComponent {
  static Options(title: string, acceptText: string, rejectText: string) {
    return {title: title, acceptText: acceptText, rejectText: rejectText};
  }

  static OpenWait(dialog: MatDialog, title: string, acceptText: string, rejectText: string): Promise<boolean> {
    return dialog.open(ConfirmDialogComponent, {data: {title: title, acceptText: acceptText, rejectText: rejectText}})
      .afterClosed()
      .toPromise();
  }

  constructor(@Inject(MAT_DIALOG_DATA) public data: any, public dialogRef: MatDialogRef<ConfirmDialogComponent>) {
  }


  reject() {
    this.dialogRef.close();
  }
}
