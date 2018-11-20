import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SelfComponent } from './self.component';

xdescribe('SelfComponent', () => {
  let component: SelfComponent;
  let fixture: ComponentFixture<SelfComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [SelfComponent]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SelfComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
