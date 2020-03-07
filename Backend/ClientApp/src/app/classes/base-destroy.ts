import { OnDestroy } from '@angular/core';
import { MonoTypeOperatorFunction, ReplaySubject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

export class BaseDestroy implements OnDestroy {
  protected $onDestroy = new ReplaySubject<void>();

  ngOnDestroy(): void {
    this.$onDestroy.next();
    this.$onDestroy.complete();
  }

  takeUntilDestroy<T>(): MonoTypeOperatorFunction<T> {
    return takeUntil<T>(this.$onDestroy);
  }

}
