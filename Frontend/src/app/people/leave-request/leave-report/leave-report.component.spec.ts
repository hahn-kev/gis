import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { LeaveReportComponent } from './leave-report.component';

describe('LeaveReportComponent', () => {
  let component: LeaveReportComponent;
  let fixture: ComponentFixture<LeaveReportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ LeaveReportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(LeaveReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
