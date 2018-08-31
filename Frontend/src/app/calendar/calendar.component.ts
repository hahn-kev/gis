import {Component, OnInit} from '@angular/core';
import {LeaveRequestService} from '../people/leave-request/leave-request.service';
import {LeaveRequestWithNames} from '../people/leave-request/leave-request';
import * as moment from 'moment';
import {Moment} from 'moment';
import {DateModel} from './date-model';
import {UrlBindingService} from '../services/url-binding.service';
import {LeaveTypeName} from "../people/self/self";

@Component({
  selector: 'app-calendar',
  templateUrl: './calendar.component.html',
  styleUrls: ['./calendar.component.scss'],
  providers: [UrlBindingService]
})
export class CalendarComponent implements OnInit {
  leaveTypeName = LeaveTypeName;
  readonly FORMAT = 'M-D-YYYY';
  readonly URL_FORMAT = 'M-YYYY';
  // noinspection TypeScriptUnresolvedVariable
  leaveRequests: Map<string, LeaveRequestWithNames[]>;
  models: DateModel<LeaveRequestWithNames>[];
  month: Moment;

  constructor(private leaveService: LeaveRequestService, private urlBinding: UrlBindingService<{ date: string }>) {
    this.urlBinding.addParam('date', moment().format(this.URL_FORMAT))
      .subscribe(value => this.month = moment(value, this.URL_FORMAT));
    this.urlBinding.loadFromParams();
    this.leaveService.list().subscribe(value => {
      this.groupLeaveRequestsByDate(value);
      this.generateModel();
    });
  }

  groupLeaveRequestsByDate(leaveRequests: LeaveRequestWithNames[]) {
    // noinspection TypeScriptUnresolvedVariable
    this.leaveRequests = new Map<string, LeaveRequestWithNames[]>();
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
      this.models[i - 1] = new DateModel<LeaveRequestWithNames>(date,
        this.leaveRequests.get(date.format(this.FORMAT)) || [],
        this.month.month() == date.month());
    }
  }

  incrementMonth(amount: number) {
    this.month.add(amount, 'month');
    this.urlBinding.values.date = this.month.format(this.URL_FORMAT);
    this.generateModel();
  }

  ngOnInit() {
  }

}
