import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DonerComponent } from './doner.component';

describe('DonerComponent', () => {
  let component: DonerComponent;
  let fixture: ComponentFixture<DonerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [DonerComponent]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DonerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
