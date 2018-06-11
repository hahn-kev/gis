import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OrgTreeComponent } from './org-tree.component';

describe('OrgTreeComponent', () => {
  let component: OrgTreeComponent;
  let fixture: ComponentFixture<OrgTreeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [OrgTreeComponent]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OrgTreeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
