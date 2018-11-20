import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AccordionListComponent } from './accordion-list.component';
import { BaseEntity } from '../../classes/base-entity';

xdescribe('AccordionListComponent', () => {
  let component: AccordionListComponent<BaseEntity>;
  let fixture: ComponentFixture<AccordionListComponent<BaseEntity>>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [AccordionListComponent]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AccordionListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
