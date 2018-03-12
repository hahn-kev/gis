import { Injectable, Injector } from '@angular/core';
import { GoogleApiService } from 'ng-gapi';
import { SettingsService } from '../services/settings.service';
import { LoginService } from '../services/auth/login.service';
// noinspection ES6UnusedImports
import {} from 'google.picker';
import 'rxjs/add/operator/first';

@Injectable()
export class DrivePickerService {
  private loaded = false;
  private activePicker: google.picker.Picker;

  constructor(private injector: Injector,
              private settings: SettingsService,
              private loginService: LoginService) {
  }

  private loadPicker() {
    if (this.loaded) return Promise.resolve();
    return new Promise<void>(resolve => {
      this.injector.get(GoogleApiService).onLoad().subscribe(() => {
        gapi.load('picker', () => {
          this.loaded = true;
          resolve();
        });
      });
    });
  }

  async openPicker() {
    const gisTeamDriveId = '0ANi-SiRomIBKUk9PVA';
    await this.loadPicker();
    const token = await this.loginService.currentUserToken().first().toPromise();
    return new Promise<any>(resolve => {
      this.activePicker = new google.picker.PickerBuilder()
        .enableFeature(google.picker.Feature.SUPPORT_TEAM_DRIVES)
        .addView(new google.picker.DocsView()
          .setParent(gisTeamDriveId)
          .setIncludeFolders(true)
        )
        .addView(new google.picker.DocsUploadView()
          .setParent(gisTeamDriveId)
          .setIncludeFolders(true)
        )
        .setOAuthToken(token.oauth)
        .setDeveloperKey(this.settings.get<string>('googleAPIKey'))
        .setCallback((data) => {
          if (data[google.picker.Response.ACTION] === google.picker.Action.PICKED) resolve(data);
          if (data[google.picker.Response.ACTION] === google.picker.Action.CANCEL) resolve(false);
        })
        .build();
      this.activePicker.setVisible(true);
    });
  }
}
