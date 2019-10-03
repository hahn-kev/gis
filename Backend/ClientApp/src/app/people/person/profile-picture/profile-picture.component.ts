import { Component, Input, OnInit } from '@angular/core';
import { PersonWithOthers } from '../../person';
import { Attachment } from '../../../attachments/attachment';

@Component({
  selector: 'app-profile-picture',
  templateUrl: './profile-picture.component.html',
  styleUrls: ['./profile-picture.component.scss']
})
export class ProfilePictureComponent implements OnInit {
  @Input() person: PersonWithOthers;
  attachment: Attachment = null;

  constructor() {
  }

  ngOnInit() {
  }

}
