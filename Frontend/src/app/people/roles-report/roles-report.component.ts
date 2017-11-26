import { Component, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RoleExtended } from '../role';
import { PersonService } from '../person.service';

@Component({
  selector: 'app-roles-report',
  templateUrl: './roles-report.component.html',
  styleUrls: ['./roles-report.component.scss']
})
export class RolesReportComponent {

  public roles: RoleExtended[];
  public during: boolean;
  public beginDate: Date;
  public endDate: Date;
  readonly oneYearInMs = 1000 * 60 * 60 * 24 * 365;


  constructor(private route: ActivatedRoute,
    private router: Router,
    private personService: PersonService) {
    this.route.data.subscribe((data: { roles: RoleExtended[] }) => {
      this.roles = data.roles;
    });
    this.route.params.subscribe((params: { start }) => this.during = params.start === 'during');
    this.route.queryParams.subscribe((params: { begin, end }) => {
      this.beginDate = new Date(params.begin || Date.now() - this.oneYearInMs / 2);
      this.endDate = new Date(params.end || Date.now() + this.oneYearInMs / 2);
    });
  }

  setDuring(during: boolean): void {
  this.during = during;
  this.updateRoute();
}

  async setBeginDate(beginDate: Date): Promise<void> {
  this.beginDate = beginDate;
  this.updateRoute();
}

  async setEndDate(endDate: Date): Promise<void> {
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

  static formatDate(date: Date): string {
    return `${date.getUTCFullYear()}-${date.getUTCMonth() + 1}-${date.getUTCDate()}`;
  }
}
