import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MissionOrgComponent } from './mission-org.component';

describe('MissionOrgComponent', () => {
  let component: MissionOrgComponent;
  let fixture: ComponentFixture<MissionOrgComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MissionOrgComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MissionOrgComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
