import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RenderTemplateButtomSheetComponent } from './render-template-buttom-sheet.component';

describe('RenderTemplateButtomSheetComponent', () => {
  let component: RenderTemplateButtomSheetComponent;
  let fixture: ComponentFixture<RenderTemplateButtomSheetComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [RenderTemplateButtomSheetComponent]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RenderTemplateButtomSheetComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
