import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { HolidayComponent } from './holiday.component';
import { MockAppTemplateContentDirective } from '../../directives/app-template-content.directive.spec';

xdescribe('HolidayComponent', () => {
  let component: HolidayComponent;
  let fixture: ComponentFixture<HolidayComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [HolidayComponent, MockAppTemplateContentDirective]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(HolidayComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
