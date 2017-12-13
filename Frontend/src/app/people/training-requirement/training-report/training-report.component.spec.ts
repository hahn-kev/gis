import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TrainingReportComponent } from './training-report.component';

describe('TrainingReportComponent', () => {
  let component: TrainingReportComponent;
  let fixture: ComponentFixture<TrainingReportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TrainingReportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TrainingReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
