import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { HolidayListComponent } from './holiday-list.component';
import { MockAppTemplateContentDirective } from '../../directives/app-template-content.directive.spec';

xdescribe('HolidayListComponent', () => {
  let component: HolidayListComponent;
  let fixture: ComponentFixture<HolidayListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [HolidayListComponent, MockAppTemplateContentDirective]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(HolidayListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
