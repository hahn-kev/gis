import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RenderTemplateBottomSheetComponent } from './render-template-bottom-sheet.component';

describe('RenderTemplateBottomSheetComponent', () => {
  let component: RenderTemplateBottomSheetComponent;
  let fixture: ComponentFixture<RenderTemplateBottomSheetComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [RenderTemplateBottomSheetComponent]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RenderTemplateBottomSheetComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
