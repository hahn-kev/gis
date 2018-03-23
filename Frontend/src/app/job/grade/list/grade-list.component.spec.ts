import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GradeListComponent } from './grade-list.component';

describe('GradeListComponent', () => {
  let component: GradeListComponent;
  let fixture: ComponentFixture<GradeListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GradeListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GradeListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
