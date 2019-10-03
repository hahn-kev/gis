import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import * as moment from 'moment';
import { AppModule } from './app/app.module';
import { environment } from './environments/environment';
import { hmrBootstrap } from './hmr';
import 'hammerjs';
import * as Sentry from '@sentry/browser';

Sentry.init({dsn: 'https://026d43df17b245588298bfa5ac8aa333@sentry.io/249854', environment: 'production'});

if (environment.production) {
  enableProdMode();
}
moment.fn.toJSON = function () {
  return this.format('YYYY-MM-DD[T]HH:mm:ss.SSS');
};

const bootstrap = () => platformBrowserDynamic().bootstrapModule(AppModule);
if (environment.hmr) {
  if (module['hot']) {
    hmrBootstrap(module, bootstrap);
  } else {
    console.error('Ammm.. HMR is not enabled for webpack');
  }
} else {
  bootstrap();
}
