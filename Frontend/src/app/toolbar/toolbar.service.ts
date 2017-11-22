import { Injectable, TemplateRef } from '@angular/core';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

@Injectable()
export class ToolbarService {
  templateRefSubject = new BehaviorSubject<TemplateRef<any>>(null);

  constructor() {
  }

  public setTemplate(template: TemplateRef<any>) {
    this.templateRefSubject.next(template);
  }

  public ObserveTemplateRef() {
    return this.templateRefSubject.asObservable();
  }
}
