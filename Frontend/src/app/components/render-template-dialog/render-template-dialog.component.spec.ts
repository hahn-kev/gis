import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RenderTemplateDialogComponent } from './render-template-dialog.component';

describe('RenderTemplateDialogComponent', () => {
  let component: RenderTemplateDialogComponent;
  let fixture: ComponentFixture<RenderTemplateDialogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RenderTemplateDialogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RenderTemplateDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
