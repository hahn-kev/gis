import { Moment } from 'moment';


export class DateModel<T> {

  constructor(date: Moment, items: T[], public inMonth: boolean) {
    this.date = date;
    this.items = items;
  }

  date: Moment;
  items: T[];

  get dayOfMonth() {
    return this.date.date();
  }

  get monthName() {
    return this.date.format('MMM');
  }
}
