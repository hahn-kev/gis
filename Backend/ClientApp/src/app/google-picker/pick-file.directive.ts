import { Directive, EventEmitter, HostListener, Output } from '@angular/core';
import { Attachment } from '../attachments/attachment';
import { PickerResponse } from './picker-response';
import { DrivePickerService } from './drive-picker.service';

@Directive({
  selector: '[appPickFile]'
})
export class PickFileDirective {
  @Output('appPickFile')
  appPickFile = new EventEmitter<Attachment>();

  public result: PickerResponse;

  constructor(private driveService: DrivePickerService) {
  }

  @HostListener('click')
  invokePicker() {
    this.driveService.openPicker()
      .then(result => {
        this.result = result;
        if (this.result)
          this.appPickFile.emit(this.driveService.convertToAttachment(this.result.documents[0]));
      });
  }
}
