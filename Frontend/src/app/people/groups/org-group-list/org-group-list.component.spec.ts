import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OrgGroupListComponent } from './org-group-list.component';

describe('OrgGroupListComponent', () => {
  let component: OrgGroupListComponent;
  let fixture: ComponentFixture<OrgGroupListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [OrgGroupListComponent]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OrgGroupListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
