import { Component, OnInit } from '@angular/core';
import { of } from 'rxjs';
import { AttachmentService } from './attachment.service';
import { Attachment } from './attachment';
import { MatDialog } from '@angular/material/dialog';
import { switchMap, tap } from 'rxjs/operators';
import { ConfirmDialogComponent } from '../dialog/confirm-dialog/confirm-dialog.component';
import { BaseDestroy } from '../classes/base-destroy';

@Component({
  selector: 'app-attachments',
  templateUrl: './attachments.component.html',
  styleUrls: ['./attachments.component.scss']
})
export class AttachmentsComponent extends BaseDestroy implements OnInit {
  attachments: Attachment[];
  attachedToId: string;

  constructor(private attachmentService: AttachmentService, private dialog: MatDialog) {
    super();
  }

  ngOnInit() {
    this.attachmentService.extractId()
      .pipe(
        tap(value => this.attachedToId = value.id),
        switchMap((value) => {
          if (value.hasAttachments) return this.attachmentService.attachedOn(value.id);
          return of([]);
        }),
        this.takeUntilDestroy())
      .subscribe(attachments => {
        this.attachments = attachments;
      });
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
}
