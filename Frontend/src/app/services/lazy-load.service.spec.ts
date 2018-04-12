import { TestBed, inject } from '@angular/core/testing';

import { LazyLoadService } from './lazy-load.service';
import { Observable } from 'rxjs/Observable';

describe('LazyLoadService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [LazyLoadService]
    });

  });

  it('should be created', inject([LazyLoadService], (service: LazyLoadService) => {
    expect(service).toBeTruthy();
  }));

  function createObservable(spy) {
    return new Observable<string>(subscriber => {
      spy();
      subscriber.next('first value');
      subscriber.complete();
    });
  }

  it('should share the result of multiple observables',
    inject([LazyLoadService], (service: LazyLoadService) => {
      let sourceSpy = jasmine.createSpy('sourceSpy');
      let destinationSpy = jasmine.createSpy('destSpy');

      service.share('123', () => createObservable(sourceSpy)).subscribe(value => {
        destinationSpy(value);
      });
      service.share('123', () => createObservable(sourceSpy)).subscribe(value => {
        destinationSpy(value);
      });

      expect(sourceSpy).toHaveBeenCalledTimes(1);
      expect(destinationSpy).toHaveBeenCalledTimes(2);
    }));

  it('should late subscribe',
    inject([LazyLoadService], (service: LazyLoadService) => {
      let sourceSpy = jasmine.createSpy('sourceSpy');
      let destinationSpy = jasmine.createSpy('destSpy');

      let observable = service.share('123', () => createObservable(sourceSpy));
      expect(sourceSpy).not.toHaveBeenCalled();
      observable.subscribe(value => {
        destinationSpy(value);
      });
      observable.subscribe(value => {
        destinationSpy(value);
      });
      expect(sourceSpy).toHaveBeenCalledTimes(1);
      expect(destinationSpy).toHaveBeenCalledTimes(2);
    }));
});
