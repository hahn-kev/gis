import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StaffTrainingComponent } from './staff-training.component';

describe('StaffTrainingComponent', () => {
  let component: StaffTrainingComponent;
  let fixture: ComponentFixture<StaffTrainingComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StaffTrainingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StaffTrainingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
