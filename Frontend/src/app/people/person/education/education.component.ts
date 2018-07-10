import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Degree, Education } from './education';
import { EducationService } from './education.service';
import { countries } from '../../countries';
import { map } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { NgModel } from '@angular/forms';

@Component({
  selector: 'app-education',
  templateUrl: './education.component.html',
  styleUrls: ['./education.component.scss']
})
export class EducationComponent implements OnInit {
  degreeList = Object.keys(Degree);
  public filteredCountries: Observable<string[]>;
  @Input() personId: string;

  @Input() education: Education[];

  @Output()
  educationChange = new EventEmitter<Education[]>();

  constructor(private educationService: EducationService) {
  }

  focusedCountries(countriesControl: NgModel) {
    this.filteredCountries = countriesControl.valueChanges
      .pipe(map(value => countries.filter(country => country.toLowerCase().startsWith((value || '').toLowerCase())))
      );
  }

  ngOnInit() {

  }

  createNewEducation = () => {
    let ed = new Education();
    ed.personId = this.personId;
    return ed;
  };

  save = (ed: Education) => {
    return this.educationService.save(ed);
  };
  delete = (ed: Education) => {
    return this.educationService.deleteEducation(ed.id);
  };
}
