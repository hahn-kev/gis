import { Component, OnInit } from '@angular/core';
import { LeaveRequestService } from '../people/leave-request/leave-request.service';
import { LeaveRequestWithNames } from '../people/leave-request/leave-request';
import * as moment from 'moment';
import { Moment } from 'moment';
import { DateModel } from './date-model';

@Component({
  selector: 'app-calendar',
  templateUrl: './calendar.component.html',
  styleUrls: ['./calendar.component.scss']
})
export class CalendarComponent implements OnInit {
  readonly FORMAT = 'M-D-YYYY';
  // noinspection TypeScriptUnresolvedVariable
  leaveRequests: Map<string, LeaveRequestWithNames[]>;
  models: DateModel<LeaveRequestWithNames>[];
  month: Moment;

  constructor(private leaveService: LeaveRequestService) {
    this.leaveService.list().subscribe(value => {
      this.groupLeaveRequestsByDate(value);
      this.generateModel();
    });
    this.month = moment().date(1);
  }

  groupLeaveRequestsByDate(leaveRequests: LeaveRequestWithNames[]) {
    // noinspection TypeScriptUnresolvedVariable
    this.leaveRequests = new Map<string, LeaveRequestWithNames[]>();
    for (let req of leaveRequests) {
      let startDate = moment(req.startDate);
      for (let i = 0; i < req.days; i++) {
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

    this.models = new Array(this.month.daysInMonth());
    for (let i = 1; i <= this.month.daysInMonth(); i++) {
      let date = this.month.clone().date(i);
      this.models[i - 1] = new DateModel<LeaveRequestWithNames>(date, this.leaveRequests.get(date.format(this.FORMAT)) || []);
    }
  }

  incrementMonth(amount: number) {
    this.month.add(amount, 'month');
    this.generateModel();
  }

  ngOnInit() {
  }

}
