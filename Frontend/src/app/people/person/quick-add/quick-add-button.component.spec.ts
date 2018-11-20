import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { QuickAddButtonComponent } from './quick-add-button.component';

xdescribe('QuickAddButtonComponent', () => {
  let component: QuickAddButtonComponent;
  let fixture: ComponentFixture<QuickAddButtonComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [QuickAddButtonComponent]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(QuickAddButtonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
