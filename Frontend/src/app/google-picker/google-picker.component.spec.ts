import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GooglePickerComponent } from './google-picker.component';

describe('GooglePickerComponent', () => {
  let component: GooglePickerComponent;
  let fixture: ComponentFixture<GooglePickerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [GooglePickerComponent]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GooglePickerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
