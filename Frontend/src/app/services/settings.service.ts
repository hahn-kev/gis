import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as Raven from 'raven-js';

@Injectable()
export class SettingsService {
  settings: any;

  constructor() {
  }

  //this is called by the settingsServiceFunction
  setSettings(settings: any) {
    this.settings = settings;
    Raven.setRelease(this.get<string>('version'));
  }

  get<T>(name: string, defaultValue?: T): T {
    return this.settings[name] || defaultValue;
  }
}

export function configureSettings(http: HttpClient, settingsService: SettingsService) {
  return async () => settingsService.setSettings(await http.get<any>('/api/settings').toPromise());
}
