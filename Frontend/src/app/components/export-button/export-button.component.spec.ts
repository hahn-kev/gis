import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ExportButtonComponent } from './export-button.component';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { CsvService } from '../../services/csv.service';

describe('ExportButtonComponent', () => {
  let component: ExportButtonComponent;
  let fixture: ComponentFixture<ExportButtonComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ExportButtonComponent],
      providers: [
        {
          provide: CsvService,
          useValue: jasmine.createSpyObj('CsvService', ['saveToCsv'])
        }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ExportButtonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
