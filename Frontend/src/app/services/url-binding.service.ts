import { Injectable } from '@angular/core';
import { ActivatedRoute, ParamMap, Params, Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { combineLatest } from 'rxjs/observable/combineLatest';
import { Subscription } from 'rxjs/Subscription';
import { isArray } from 'util';
import { debounceTime } from 'rxjs/operators';

@Injectable()
export class UrlBindingService<T_VALUES> {
  private _values: T_VALUES = null;
  public get values(): T_VALUES {
    if (this._values) return this._values;
    this._values = {} as T_VALUES;
    for (let i = 0; i < this.params.length; i++) {
      const name = this.params[i];
      const subject = this.subjects[i];
      Object.defineProperty(this._values, name, {
        get() {
          return subject.value;
        },
        set(value) {
          subject.next(value);
        },
      })
    }
    return this._values;
  };

  private params: string[] = [];
  public subjects: BehaviorSubject<any>[] = [];
  private defaultValues = [];
  private pathParams: boolean[] = [];
  private subjectsSubscription: Subscription;
  private ignoreUpdates = false;
  public onParamsUpdated = (values: T_VALUES) => {
  };

  constructor(private route: ActivatedRoute, private router: Router) {
    combineLatest(this.route.paramMap, this.route.queryParamMap)
      .pipe(debounceTime(1))
      .subscribe(([params, queryParams]) => {
        this._loadFromParams(params, queryParams);
      });
  }

  public loadFromParams() {
    return this._loadFromParams(this.route.snapshot.paramMap, this.route.snapshot.queryParamMap);
  }

  private _loadFromParams(params: ParamMap, queryParams: ParamMap) {
    this.ignoreUpdates = true;
    let hadUpdate = false;
    for (let i = 0; i < this.params.length; i++) {
      if (this.loadParam(i, queryParams)) hadUpdate = true;
      else if (this.loadParam(i, params)) hadUpdate = true;
    }

    this.ignoreUpdates = false;
    if (hadUpdate) this.onParamsUpdated(this.values);
    return hadUpdate;
  }

  private loadParam(i: number, params: ParamMap) {
    let hadUpdate = false;
    const name = this.params[i];
    if (!params.has(name)) return false;

    let values: Array<any> = params.getAll(name);
    const listOfValues = isArray(this.defaultValues[i]);
    switch (typeof this.defaultValues[i]) {
      case 'number' :
        values = values.map(value => Number(value));
        break;
      case 'boolean':
        // noinspection TsLint
        values = values.map(value => value != 'false');
        break;
    }
    const subject = this.subjects[i];
    if (listOfValues) {
      if (!this.areListsEqual(subject.value, values)) {
        subject.next(values);
        hadUpdate = true;
      }
    } else {
      const value = values[0];
      if (subject.value != value) {
        subject.next(value);
        hadUpdate = true;
      }
    }

    return hadUpdate;
  }

  areListsEqual(a: any[], b: any[]) {
    if (a === b) return true;
    if (a == null || b == null) return false;
    if (a.length !== b.length) return false;

    // If you don't care about the order of the elements inside
    // the array, you should sort both arrays here.
    a = [...a];
    b = [...b];
    a.sort();
    b.sort();

    for (let i = 0; i < a.length; ++i) {
      if (a[i] !== b[i]) return false;
    }
    return true;
  }

  addParam<T_VALUE>(name: string, defaultValue: T_VALUE, pathParam = false) {
    this.params.push(name);
    this.pathParams.push(pathParam);
    this.defaultValues.push(defaultValue);
    const subject = new BehaviorSubject<T_VALUE>(defaultValue);
    this.subjects.push(subject);
    this.ignoreUpdates = true;
    this.updateSubscription();
    this.ignoreUpdates = false;
    return subject.asObservable();
  }

  private updateSubscription() {
    if (this.subjectsSubscription) this.subjectsSubscription.unsubscribe();
    this._values = null;
    this.subjectsSubscription = combineLatest(this.subjects).subscribe(values => {
      if (this.ignoreUpdates) return;
      const params: Params = {};
      let commands = ['.'];
      for (let i = 0; i < this.params.length; i++) {
        const value = values[i];
        const defaultValue = this.defaultValues[i];
        const listOfValues = isArray(defaultValue);
        if (this.pathParams[i]) {
          if (!this.route.snapshot.paramMap.has(this.params[i])) {
            commands = [...commands, value];
          } else if (this.route.snapshot.paramMap.get(this.params[i]) != value) {
            commands = commands.length === 1 ? ['..', value] : ['..', ...commands, value];
          }
        } else if (listOfValues ? !this.areListsEqual(value, defaultValue) : value !== defaultValue) {
          params[this.params[i]] = value;
        }
      }
      this.router.navigate(commands, {
        relativeTo: this.route,
        queryParams: params
      });
      this.onParamsUpdated(this.values);
    });
  }
}