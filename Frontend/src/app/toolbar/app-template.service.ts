import { Injectable, TemplateRef } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable()
export class AppTemplateService {
  templateSubjects: { [key: string]: BehaviorSubject<TemplateRef<any>> } = {};

  constructor() {
  }

  private subject(name: string): BehaviorSubject<TemplateRef<any>> {
    if (!this.templateSubjects[name]) {
      this.templateSubjects[name] = new BehaviorSubject<TemplateRef<any>>(null);
    }
    return this.templateSubjects[name];
  }

  public setTemplate(name: string, template: TemplateRef<any>) {
    this.subject(name).next(template);
  }

  public ObserveTemplateRef(name: string) {
    return this.subject(name).asObservable();
  }
}
