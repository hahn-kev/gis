import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EndorsementComponent } from './endorsement.component';

xdescribe('EndorsementComponent', () => {
  let component: EndorsementComponent;
  let fixture: ComponentFixture<EndorsementComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [EndorsementComponent]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EndorsementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
