import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { DrivePickerService } from './drive-picker.service';
import { Attachment } from '../attachments/attachment';
import { PickerResponse } from './picker-response';

@Component({
  selector: 'app-google-picker',
  templateUrl: './google-picker.component.html',
  styleUrls: ['./google-picker.component.scss']
})
export class GooglePickerComponent implements OnInit {

  public result: PickerResponse;

  @Output()
  fileAttached = new EventEmitter<Attachment>();

  constructor(private driveService: DrivePickerService) {
  }

  ngOnInit() {
    this.driveService.loadPicker();
  }


  invokePicker() {
    this.driveService.signIn()
      .then(() => this.driveService.openPicker())
      .then((result) => {
        this.result = result;
        if (result)
          this.fileAttached.emit(this.driveService.convertToAttachment(this.result.documents[0]));
      });
  }
}
