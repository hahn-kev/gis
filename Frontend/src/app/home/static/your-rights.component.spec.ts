import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { YourRightsComponent } from './your-rights.component';

describe('YourRightsComponent', () => {
  let component: YourRightsComponent;
  let fixture: ComponentFixture<YourRightsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [YourRightsComponent]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(YourRightsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should be created', () => {
    expect(component).toBeTruthy();
  });
});
