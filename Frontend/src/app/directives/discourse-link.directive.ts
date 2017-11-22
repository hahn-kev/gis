import { Directive, HostBinding, Input, OnDestroy, OnInit } from '@angular/core';
import { SettingsService } from '../services/settings.service';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Subscription } from 'rxjs/Subscription';
import { combineLatest } from 'rxjs/operators';

@Directive({
  selector: '[discourseLink]'
})
export class DiscourseLinkDirective implements OnDestroy, OnInit {
  ngOnInit(): void {
    this.subscription = this.link.pipe(combineLatest(this.settingsService.getAsync<string>('discourseBaseUrl'), (link, baseUrl) => {
      return baseUrl + link;
    }))
      .subscribe(link => {
        this.href = link
      });
  }

  link = new BehaviorSubject<string>(null);
  subscription: Subscription;

  constructor(private settingsService: SettingsService) {

  }

  @HostBinding() href: string;

  @Input() set discourseLink(link: string) {
    if (link.indexOf('/') != 0) {
      link = '/' + link;
    }
    if (this.href == '') {

    }
    this.link.next(link);
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
}
