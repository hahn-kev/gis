import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { SchoolAidListComponent } from './school-aid-list.component';

xdescribe('PeopleListComponent', () => {
  let component: SchoolAidListComponent;
  let fixture: ComponentFixture<SchoolAidListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [SchoolAidListComponent]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SchoolAidListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
