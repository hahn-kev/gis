import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { ActivityIndicatorService } from './activity-indicator.service';
import { tap } from 'rxjs/operators';

@Injectable()
export class ActivityIndicatorInterceptorService implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    this.activityIndicatorService.showIndicator();
    return next.handle(req).pipe(tap(event => {
    }, error => {
      this.activityIndicatorService.hideIndicator();
    }, () => {
      this.activityIndicatorService.hideIndicator();
    }));
  }

  constructor(private activityIndicatorService: ActivityIndicatorService) {
  }

}
