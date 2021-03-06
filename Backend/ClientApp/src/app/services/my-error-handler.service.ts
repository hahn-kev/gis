import { ErrorHandler, Injectable, Injector, NgZone } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HttpErrorResponse } from '@angular/common/http';
import * as Sentry from '@sentry/browser';
import { environment } from '../../environments/environment';

@Injectable()
export class MyErrorHandlerService implements ErrorHandler {

  private original = new ErrorHandler();
  private snackBarService: MatSnackBar;
  private lastTime: Date;

  constructor(private injector: Injector, private ngZone: NgZone) {

  }

  timeSinceLast() {
    if (this.lastTime == null) {
      this.lastTime = new Date();
      return Number.MAX_SAFE_INTEGER;
    }
    let previousTime = this.lastTime;
    this.lastTime = new Date();
    return this.lastTime.getTime() - previousTime.getTime();
  }

  handleError(error: any): void {
    let message: string;
    if (error.rejection) {
      if (error.rejection instanceof HttpErrorResponse) {
        message = MyErrorHandlerService.getHttpError(error.rejection);
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
    this.showSnackbar(message);

    if (!(error.rejection instanceof HttpErrorResponse) && !(error instanceof HttpErrorResponse) && environment.production) {
      //don't report http errors, the server will report those
      Sentry.captureException(error.originalError || error);
    }
    this.original.handleError(error);
  }

  showSnackbar(message: string) {
    try {
      if (!this.snackBarService) {
        //this error handler gets created very early, so we must inject services here
        this.snackBarService = this.injector.get(MatSnackBar);
      }
    } catch (e) {
      this.original.handleError(e);
      console.error('Unable to inject snackbar service');
      return;
    }

    if (NgZone.isInAngularZone()) {
      this.snackBarService.open(message, 'Dismiss');
    } else if (this.timeSinceLast() > 100) {
      this.ngZone.run(() => {
        this.snackBarService.open(message, 'Dismiss');
      });
    }
  }

  static getHttpError(rejection: HttpErrorResponse): string {
    if (rejection.error === null) return rejection.message;
    if (rejection.error.message) return rejection.error.message;
    if (rejection.error.error && rejection.error.error.message) return rejection.error.error.message;
    console.log('http error response format unknown');
    return rejection.message;
  }

}
