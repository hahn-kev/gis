import { Injectable, Injector } from '@angular/core';
import { GoogleApiService } from 'ng-gapi';
import { SettingsService } from '../services/settings.service';
// noinspection ES6UnusedImports
import {} from 'google.picker';
import { Attachment } from '../attachments/attachment';
import { PickerDocument, PickerResponse } from './picker-response';

@Injectable()
export class DrivePickerService {
  private loaded = false;
  private activePicker: google.picker.Picker;

  constructor(private injector: Injector,
              private settings: SettingsService) {
  }

  public signIn() {
    let authInstance = gapi.auth2.getAuthInstance();
    if (authInstance.isSignedIn.get()) return Promise.resolve();
    return authInstance.signIn()
      .then(() => {
        return authInstance.isSignedIn.get();
      });
  }

  public async loadPicker() {
    if (this.loaded) return;
    let googleApiService = this.injector.get(GoogleApiService);
    await googleApiService.onLoad().toPromise();
    await new Promise<void>(resolve => {
      gapi.load('client:picker:auth2', () => {
        resolve();
      });
    });
    // @ts-ignore
    await gapi.auth2.init(googleApiService.getConfig().getClientConfig());
    this.loaded = true;
  }

  async openPicker(): Promise<PickerResponse> {
    const gisTeamDriveId = '0ANi-SiRomIBKUk9PVA';
    await this.loadPicker();
    return new Promise<PickerResponse>(resolve => {
      let authInstance = gapi.auth2.getAuthInstance();
      let googleUser = authInstance.currentUser.get();

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
        .setOAuthToken(googleUser.getAuthResponse().access_token)
        .setDeveloperKey(this.settings.get<string>('googleAPIKey'))
        .setCallback((data) => {
          if (data[google.picker.Response.ACTION] === google.picker.Action.PICKED) resolve(new PickerResponse(data));
          if (data[google.picker.Response.ACTION] === google.picker.Action.CANCEL) resolve(null);
        })
        .build();
      this.activePicker.setVisible(true);
    });
  }

  /*
  {
  "action": "picked",
  "viewToken": [
    "all",
    null,
    {
      "parent": "0ANi-SiRomIBKUk9PVA",
      "includeFolders": true
    }
  ],
  "docs": [
    {
      "id": "1mII6lISg4uEKJNkXMXeT_lKV96L1UxIR",
      "serviceId": "docs",
      "mimeType": "image/png",
      "name": "Nyan.png",
      "description": "",
      "type": "photo",
      "lastEditedUtc": 1520560551111,
      "iconUrl": "https://drive-thirdparty.googleusercontent.com/16/type/image/png",
      "url": "https://drive.google.com/file/d/1mII6lISg4uEKJNkXMXeT_lKV96L1UxIR/view?usp=drive_web",
      "embedUrl": "https://drive.google.com/file/d/1mII6lISg4uEKJNkXMXeT_lKV96L1UxIR/preview?usp=drive_web",
      "sizeBytes": 26158,
      "teamDriveId": "1uC90-ts_5E1173eEEsaXGZpXzVGrZx9Qm28",
      "rotation": 0,
      "rotationDegree": 0,
      "parentId": "0ANi-SiRomIBKUk9PVA",
      "isShared": true
    }
  ]
}
   */

  //above is an example result, the first doc is what gets returned
  convertToAttachment(doc: PickerDocument): Attachment {
    let attachment = new Attachment();
    attachment.googleId = doc.ID;
    attachment.name = doc.NAME;
    attachment.downloadUrl = doc.URL;
    attachment.fileType = doc.TYPE;
    return attachment;

  }
}
