import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, ChildActivationEnd, Router } from '@angular/router';
import { hasOwnProperty } from 'tslint/lib/utils';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/filter';
import { AttachmentService } from './attachment.service';
import 'rxjs/add/operator/mergeMap';
import { Attachment } from './attachment';
import { MatDialog } from '@angular/material';
import { ConfirmDialogComponent } from '../../dialog/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-attachments',
  templateUrl: './attachments.component.html',
  styleUrls: ['./attachments.component.scss']
})
export class AttachmentsComponent implements OnInit {
  attachments: Attachment[];
  attachedToId: string;

  constructor(private attachmentService: AttachmentService, private dialog: MatDialog) {
    this.attachmentService.extractId().subscribe(value => this.attachedToId = value.id);
    attachmentService.extractId().flatMap((value) => {
      if (value.hasAttachments) return attachmentService.attachedOn(value.id);
      return Observable.of([]);
    })
      .subscribe(attachments => {
        this.attachments = attachments;
      });
  }

  ngOnInit() {
  }

  async removeAttachment(attachment: Attachment) {
    let result = await ConfirmDialogComponent.OpenWait(this.dialog,
      `Delete Attachment ${attachment.name}?`,
      'Delete',
      'Cancel');
    if (!result) return;
    await this.attachmentService.removeAttachment(attachment.id).toPromise();
    this.attachments = this.attachments.filter(value => value.id != attachment.id);
  }

  async newAttachment(attachment: Attachment) {
    attachment.attachedToId = this.attachedToId;
    attachment = await this.attachmentService.attach(attachment).toPromise();
    this.attachments = [attachment, ...this.attachments];
  }

  getRandomIntInclusive(min, max) {
    min = Math.ceil(min);
    max = Math.floor(max);
    return Math.floor(Math.random() * (max - min + 1)) + min; //The maximum is inclusive and the minimum is inclusive
  }
}
