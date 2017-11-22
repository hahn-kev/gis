import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { LifeLessonsComponent } from './life-lessons.component';

describe('LifeLessonsComponent', () => {
  let component: LifeLessonsComponent;
  let fixture: ComponentFixture<LifeLessonsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [LifeLessonsComponent]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(LifeLessonsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should be created', () => {
    expect(component).toBeTruthy();
  });
});
