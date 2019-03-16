import { Component, OnInit } from '@angular/core';
import { CalendarLeaveRequest } from '../people/leave-request/leave-request';
import * as moment from 'moment';
import { Moment } from 'moment';
import { DateModel } from './date-model';
import { UrlBindingService } from '../services/url-binding.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-calendar',
  templateUrl: './calendar.component.html',
  styleUrls: ['./calendar.component.scss'],
  providers: [UrlBindingService]
})
export class CalendarComponent implements OnInit {
  readonly FORMAT = 'M-D-YYYY';
  readonly URL_FORMAT = 'M-YYYY';
  leaveRequests: Map<string, CalendarLeaveRequest[]>;
  models: DateModel<CalendarLeaveRequest>[];
  month: Moment;
  public allOrgGroups: string[] = [];

  constructor(private route: ActivatedRoute, private urlBinding: UrlBindingService<{ date: string, group: string[] }>) {
  }

  ngOnInit() {
    this.urlBinding.addParam('date', moment().format(this.URL_FORMAT))
      .subscribe(value => this.month = moment(value, this.URL_FORMAT));
    this.urlBinding.addParam('group', [])
      .subscribe(() => this.generateModel());
    this.urlBinding.loadFromParams();
    this.route.data.subscribe((value: { leave: CalendarLeaveRequest[] }) => {
      this.groupLeaveRequestsByDate(value.leave);
      this.generateModel();
    });
  }

  groupLeaveRequestsByDate(leaveRequests: CalendarLeaveRequest[]) {
    this.leaveRequests = new Map<string, CalendarLeaveRequest[]>();
    this.allOrgGroups = leaveRequests
      .map(value => value.orgGroupName)
      .filter((value, index, array) => array.indexOf(value) == index && value != null)
      .sort();
    for (let req of leaveRequests) {
      let startDate = moment(req.startDate);
      let endDate = moment(req.endDate);
      let days = endDate.diff(startDate, 'd') + 1;
      for (let i = 0; i < days; i++) {
        let date = startDate.clone().add(i, 'days').format(this.FORMAT);
        if (this.leaveRequests.has(date)) {
          this.leaveRequests.get(date).push(req);
        } else {
          this.leaveRequests.set(date, [req]);
        }
      }
    }
  }

  generateModel() {
    if (!this.leaveRequests) return;
    let rows = 5;
    let columns = 7;
    let monthStartWeekday = this.month.weekday();
    //calculate if we need an extra row for this month
    if (monthStartWeekday + (this.month.daysInMonth() % 7) > 7) {
      rows++;
    }
    let offset = monthStartWeekday;
    this.models = new Array(rows * columns);
    for (let i = 1; i <= this.models.length; i++) {
      let date = this.month.clone().date(i - offset);
      let requests = this.leaveRequests.get(date.format(this.FORMAT)) || [];
      if (this.urlBinding.values.group.length > 0) {
        requests = requests.filter(request => this.urlBinding.values.group.includes(request.orgGroupName));
      }
      this.models[i - 1] = new DateModel<CalendarLeaveRequest>(date,
        requests,
        this.month.month() == date.month());
    }
  }

  incrementMonth(amount: number) {
    this.month.add(amount, 'month');
    this.urlBinding.values.date = this.month.format(this.URL_FORMAT);
    this.generateModel();
  }

}
