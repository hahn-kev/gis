import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MissionOrgListComponent } from './mission-org-list.component';

xdescribe('MissionOrgListComponent', () => {
  let component: MissionOrgListComponent;
  let fixture: ComponentFixture<MissionOrgListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MissionOrgListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MissionOrgListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
