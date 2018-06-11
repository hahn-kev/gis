import { ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ConfirmDialogComponent } from '../dialog/confirm-dialog/confirm-dialog.component';
import { CanComponentDeactivate } from '../services/can-deactivate.guard';
import { MatDialog } from '@angular/material';

export class BaseEditComponent implements CanComponentDeactivate {
  @ViewChild(NgForm) form: NgForm;
  formSubmitted = false;

  constructor(protected dialog: MatDialog) {

  }

  canDeactivate() {
    if (!this.form || this.form.pristine || this.form.submitted || this.formSubmitted) return true;
    return ConfirmDialogComponent.OpenWait(this.dialog, 'Discard Changes?', 'Discard', 'Cancel');
  }
}
