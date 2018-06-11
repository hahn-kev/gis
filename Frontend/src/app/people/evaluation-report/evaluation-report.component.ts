import { Component, OnInit, ViewChild } from '@angular/core';
import { AppDataSource } from '../../classes/app-data-source';
import { PersonEvluationSummary } from './person-evluation-summary';
import { UrlBindingService } from '../../services/url-binding.service';
import { ActivatedRoute } from '@angular/router';
import { MatBottomSheet, MatSort } from '@angular/material';
import { EvaluationService } from '../person/evaluation/evaluation.service';
import { PersonWithStaff } from '../person';
import { RenderTemplateBottomSheetComponent } from '../../components/render-template-bottom-sheet/render-template-bottom-sheet.component';

@Component({
  selector: 'app-evaluation-report',
  templateUrl: './evaluation-report.component.html',
  styleUrls: ['./evaluation-report.component.scss'],
  providers: [UrlBindingService]
})
export class EvaluationReportComponent implements OnInit {
  public dataSource = new AppDataSource<PersonEvluationSummary>();
  public allOrgGroups: string[] = [];
  @ViewChild(MatSort) public sort: MatSort;

  constructor(public urlBinding: UrlBindingService<{ search: string, show: string[], group: string[] }>,
              private route: ActivatedRoute,
              private evaluationService: EvaluationService,
              private bottomSheet: MatBottomSheet) {
    this.urlBinding.addParam('search', '').subscribe(value => this.dataSource.filter = value.toUpperCase());
    this.urlBinding.addParam('show', ['thai', 'nonThai']);
    this.urlBinding.addParam('group', []);
    this.dataSource.bindToRouteData(this.route, 'evaluationSummary');
    //filter list to distinct
    this.allOrgGroups = this.dataSource.filteredData
      .map(value => value.person.staff.orgGroupName)
      .filter((value, index, array) => array.indexOf(value) == index && value != null);

    this.dataSource.filterPredicate = (data, filter) =>
      data.person.firstName.toUpperCase().startsWith(filter)
      || data.person.lastName.toUpperCase().startsWith(filter)
      || data.person.preferredName.startsWith(filter);
    this.dataSource.customFilter = value => {
      if (value.person.isThai && !this.urlBinding.values.show.includes('thai')) return false;
      if (!value.person.isThai && !this.urlBinding.values.show.includes('nonThai')) return false;
      if (this.urlBinding.values.group.length > 0
        && !this.urlBinding.values.group.includes(value.person.staff.orgGroupName)) return false;
      return true;
    };
    this.urlBinding.onParamsUpdated = values => {
      this.dataSource.filterUpdated();
    };
    if (!this.urlBinding.loadFromParams()) {
      this.dataSource.filterUpdated();
    }
  }

  ngOnInit() {
    this.dataSource.sort = this.sort;
  }

  async openEvaluationList(person: PersonWithStaff) {
    let evaluations = await this.evaluationService.getEvaluationsByPersonId(person.id).toPromise();
    RenderTemplateBottomSheetComponent.Open(this.bottomSheet,
      'evaluationList',
      {person: person, evaluations: evaluations});
  }

}
