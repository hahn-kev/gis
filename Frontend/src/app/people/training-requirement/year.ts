import * as moment from 'moment';
import { MomentInput } from 'moment';

export class Year {

  constructor(public value: number, public name: string) {
  }

  get startOfYear() {
    //month is 0 based, 7 is august
    return new Date(this.value, 7, 1);
  }

  get endOfYear() {
    //month is 0 based, 4 is may
    return new Date(this.value + 1, 4, 31);
  }

  convertToSchoolYear(date: MomentInput) {
    let mDate = moment(date).year(this.value);
    if (Year.InFirstHalf(mDate)) return mDate;
    return mDate.add(1, 'y');
  }

  static InFirstHalf(date: MomentInput) {
    return moment(date).month() > 6;
  }

  static InLastHalf(date: MomentInput) {
    return moment(date).month() <= 5;
  }

  static CurrentSchoolYear() {
    let date = moment();
    if (this.InFirstHalf(date)) return date.year();
    return date.year() - 1;
  }
}
