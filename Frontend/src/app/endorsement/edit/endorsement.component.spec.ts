import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EndorsmentComponent } from './endorsment.component';

describe('EndorsmentComponent', () => {
  let component: EndorsmentComponent;
  let fixture: ComponentFixture<EndorsmentComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EndorsmentComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EndorsmentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
