import { Injectable } from '@angular/core';
import { ActivationEnd, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { Attachment } from './attachment';
import { HttpClient } from '@angular/common/http';
import { filter, map, shareReplay } from 'rxjs/operators';

@Injectable()
export class AttachmentService {
  eventsTransformed: Observable<{ id: string | null, hasAttachments: boolean }>;

  constructor(private router: Router, private http: HttpClient) {
    this.eventsTransformed = router.events.pipe(filter(value => value instanceof ActivationEnd),
      map(value => (<ActivationEnd > value).snapshot),
      filter(snapshot => snapshot.firstChild === null),
      map(snapshot => {
        let idParameter = snapshot.data.hasOwnProperty('idParameter') ? snapshot.data['idParameter'] : 'id';
        let hasAttachments = snapshot.data.hasOwnProperty('hasAttachments') ? snapshot.data['hasAttachments'] : true;
        let id = snapshot.paramMap.has(idParameter) ? snapshot.paramMap.get(idParameter) : null;
        //guid: 9f830bab-0edb-4170-85f6-1906ab4205c4
        if (!/.{8}-.{4}-.{4}-.{12}/.test(id)) id = null;
        return {id: id, hasAttachments: hasAttachments && id !== null};
      }),
      shareReplay());
  }


  extractId(): Observable<{ id: string | null, hasAttachments: boolean }> {
    return this.eventsTransformed;
  }

  attachedOn(attachedToId: string) {
    return this.http.get<Attachment[]>('/api/attachment/on/' + attachedToId);
  }

  attach(attachment: Attachment) {
    return this.http.post<Attachment>('/api/attachment', attachment);
  }

  removeAttachment(attachmentId: string) {
    return this.http.delete('/api/attachment/' + attachmentId, {responseType: 'text'});
  }
}
