import { Component, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RoleExtended, RoleWithJob } from '../role';
import { PersonService } from '../person.service';
import * as moment from 'moment';
import { Moment } from 'moment';

@Component({
  selector: 'app-roles-report',
  templateUrl: './roles-report.component.html',
  styleUrls: ['./roles-report.component.scss']
})
export class RolesReportComponent {

  public roles: RoleWithJob[];
  public during: boolean;
  public beginDate: Moment;
  public endDate: Moment;
  readonly oneYearInMs = 1000 * 60 * 60 * 24 * 365;


  constructor(private route: ActivatedRoute,
              private router: Router,
              private personService: PersonService) {
    this.route.data.subscribe((data: { roles: RoleWithJob[] }) => {
      this.roles = data.roles;
    });
    this.route.params.subscribe((params: { start }) => this.during = params.start === 'during');
    this.route.queryParams.subscribe((params: { begin, end }) => {
      this.beginDate = this.parseDate(params.begin, Date.now() - this.oneYearInMs / 2);
      this.endDate = this.parseDate(params.end, Date.now() + this.oneYearInMs / 2);
    });
  }

  parseDate(param: string, fallback: number) {
    if (param) return moment(param, 'YYYY-M-D');
    return moment(fallback);
  }

  setDuring(during: boolean): void {
    this.during = during;
    this.updateRoute();
  }

  async setBeginDate(beginDate: Moment): Promise<void> {
    this.beginDate = beginDate;
    this.updateRoute();
  }

  async setEndDate(endDate: Moment): Promise<void> {
    this.endDate = endDate;
    this.updateRoute();
  }

  updateRoute(): void {
    this.router.navigate(['..', this.during ? 'during' : 'before'],
      {
        relativeTo: this.route,
        queryParams: {
          begin: RolesReportComponent.formatDate(this.beginDate),
          end: RolesReportComponent.formatDate(this.endDate)
        },
      });
  }

  static formatDate(date: Moment): string {
    return date.format('YYYY-M-D');
  }
}
