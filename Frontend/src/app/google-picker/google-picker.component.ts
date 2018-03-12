import { Component, OnInit } from '@angular/core';
import { DrivePickerService } from './drive-picker.service';

@Component({
  selector: 'app-google-picker',
  templateUrl: './google-picker.component.html',
  styleUrls: ['./google-picker.component.scss']
})
export class GooglePickerComponent implements OnInit {

  public result: any;

  constructor(private driveService: DrivePickerService) {
  }

  ngOnInit() {
  }

  async invokePicker() {
    this.result = await this.driveService.openPicker();
  }
}
