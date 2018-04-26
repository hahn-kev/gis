import * as moment from 'moment';
import { MomentInput } from 'moment';

export class Year {


  static InFirstHalf(date: MomentInput) {
    return moment(date).month() > 6;
  }

  static InLastHalf(date: MomentInput) {
    return moment(date).month() <= 5;
  }

  static CurrentSchoolYear() {
    const date = moment();
    if (this.InFirstHalf(date)) return date.year();
    return date.year() - 1;
  }

  static years(): Year[] {
    const today = new Date();
    const years = new Array<Year>(today.getUTCFullYear() - 2000 + 3);
    for (let i = 0; i < years.length; i++) {
      let display;
      if (i < 9) {
        display = `0${i} - 0${i + 1}`;
      } else if (i === 9) {
        display = '09 - 10';
      } else {
        display = `${i} - ${i + 1}`;
      }
      years[i] = new Year(i + 2000, display);
    }
    return years.reverse();
  }

  constructor(public value: number, public name?: string) {
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
    const mDate = moment(date).year(this.value);
    if (Year.InFirstHalf(mDate)) return mDate;
    return mDate.add(1, 'y');
  }
}
