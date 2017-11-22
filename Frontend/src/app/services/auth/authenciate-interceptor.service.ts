import { Injectable } from '@angular/core';
import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { tap } from 'rxjs/operators';
import { LoginService } from './login.service';

@Injectable()
export class AuthenciateInterceptorService implements HttpInterceptor {

  constructor(private loginService: LoginService) {
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (this.loginService.accessToken)
      req = req.clone({setHeaders: {Authorization: 'Bearer ' + this.loginService.accessToken}});
    return next.handle(req).pipe(tap(event => {
    }, e => {
      if (e instanceof HttpErrorResponse) {
        let err: HttpErrorResponse = e;
        if (err.status == 401 && !err.url.endsWith('signin')) {
          this.loginService.promptLogin();
        } else if (err.headers.has('content-type') && err.headers.get('content-type').indexOf('application/json') == 0) {
          //error is json, parse to json
          this.setError(err);
        }
      }

    }));
  }

  setError(e: any) {
    if (typeof e.error === 'string') e.error = JSON.parse(e.error);
  }
}
