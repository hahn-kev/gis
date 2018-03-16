import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, ChildActivationEnd, Router } from '@angular/router';
import { hasOwnProperty } from 'tslint/lib/utils';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/filter';
import { AttachmentService } from './attachment.service';
import 'rxjs/add/operator/mergeMap';
import { Attachment } from './attachment';

@Component({
  selector: 'app-attachments',
  templateUrl: './attachments.component.html',
  styleUrls: ['./attachments.component.scss']
})
export class AttachmentsComponent implements OnInit {
  attachments: Attachment[];
  attachedToId: string;

  constructor(private attachmentService: AttachmentService) {
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

  async removeAttachment(attachmentId: string) {
    await this.attachmentService.removeAttachment(attachmentId).toPromise();
    this.attachments = this.attachments.filter(value => value.id != attachmentId);
  }

  async newAttachment() {
    let attachment = new Attachment();
    attachment.name = 'some sample name ' + this.getRandomIntInclusive(1, 100);
    attachment.attachedToId = this.attachedToId;
    attachment.googleId = new Array(10).fill(1).map(value => {
     return String.fromCharCode(this.getRandomIntInclusive(48, 122))
    }).join("");
    attachment.downloadUrl = 'google.com/d/' + attachment.googleId;
    attachment.fileType = 'doc';
    attachment = await this.attachmentService.attach(attachment).toPromise();
    this.attachments = [attachment, ...this.attachments];
  }

  getRandomIntInclusive(min, max) {
    min = Math.ceil(min);
    max = Math.floor(max);
    return Math.floor(Math.random() * (max - min + 1)) + min; //The maximum is inclusive and the minimum is inclusive
  }
}
