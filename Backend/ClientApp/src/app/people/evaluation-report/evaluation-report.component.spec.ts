import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EvaluationReportComponent } from './evaluation-report.component';

xdescribe('EvaluationReportComponent', () => {
  let component: EvaluationReportComponent;
  let fixture: ComponentFixture<EvaluationReportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [EvaluationReportComponent]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EvaluationReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
