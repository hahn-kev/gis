import { ErrorHandler, Injectable, Injector } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { HttpErrorResponse } from '@angular/common/http';
import * as Raven from 'raven-js';

@Injectable()
export class MyErrorHandlerService implements ErrorHandler {
  handleError(error: any): void {
    if (!this.snackBarService) {
      //this error handeler gets created very early, so we must inject services here
      this.snackBarService = this.injector.get(MatSnackBar);
    }

    let message: string;
    if (error.rejection) {
      if (error.rejection instanceof HttpErrorResponse) {
        message = error.rejection.error.message || error.rejection.error.error.message;
      } else {
        message = error.rejection.message;
      }
    } else if (error.error && error.error.message) {
      message = error.error.message;
    } else if (error.message) {
      message = error.message;
    } else {
      message = error.toString();
    }
    let bar = this.snackBarService.open(message, 'Dismiss');
    if (!(error.rejection instanceof HttpErrorResponse) && !(error instanceof HttpErrorResponse)) {
      //don't report http errors, the server will report those
      Raven.captureException(error);
    }
    this.original.handleError(error);
  }

  private original = new ErrorHandler();
  private snackBarService: MatSnackBar;

  constructor(private injector: Injector) {

  }

}
