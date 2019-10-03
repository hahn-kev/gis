import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RolesReportComponent } from './roles-report.component';

xdescribe('RolesReportComponent', () => {
  let component: RolesReportComponent;
  let fixture: ComponentFixture<RolesReportComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RolesReportComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RolesReportComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
