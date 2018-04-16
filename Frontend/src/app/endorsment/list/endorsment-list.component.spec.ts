import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EndorsmentListComponent } from './endorsment-list.component';

describe('EndorsmentListComponent', () => {
  let component: EndorsmentListComponent;
  let fixture: ComponentFixture<EndorsmentListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EndorsmentListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EndorsmentListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
