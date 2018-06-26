import * as moment from 'moment';
import { MomentInput } from 'moment';

export class Year {
  //month in base 1 for months
  static readonly schoolStartMonth = 7;
  static readonly schoolEndMonth = 6;

  static CurrentSchoolYear() {
    const date = moment();
    if ((date.month() + 1) >= Year.schoolStartMonth) return date.year();
    return date.year() - 1;
  }

  static schoolYear(date: MomentInput) {
    const momentDate = moment(date);
    return (momentDate.month() + 1) >= Year.schoolStartMonth ? momentDate.year() : momentDate.year() - 1;
  }

  static years(): Year[] {
    const today = new Date();
    const years = new Array<Year>(today.getUTCFullYear() - 2000 + 3);
    for (let i = 0; i < years.length; i++) {
      let display = Year.yearName(i + 2000);
      years[i] = new Year(i + 2000, display);
    }
    return years.reverse();
  }

  static schoolYearNameFromDate(date: MomentInput) {
    return Year.yearName(Year.schoolYear(date));
  }

  static dateRangeIntersectsWithYear(startDate: MomentInput, endDate: MomentInput | null, year: number) {
    if (endDate == null) return Year.endOfYear(year).isAfter(startDate);
    if (Year.startOfYear(year).isBetween(startDate, endDate)) return true;
    return Year.endOfYear(year).isBetween(startDate, endDate);
  }

  static yearName(year: number) {
    year -= 2000;
    if (year < 9) {
      return `0${year} - 0${year + 1}`;
    } else if (year === 9) {
      return '09 - 10';
    } else {
      return `${year} - ${year + 1}`;
    }
  }

  static startOfYear(year: number) {
    return moment(new Date(year, Year.schoolStartMonth + 1, 1));
  }

  static endOfYear(year: number) {
    return moment(new Date(year + 1, Year.schoolEndMonth + 1, 30));
  }

  constructor(public value: number, public name?: string) {
  }

  get startOfYear() {
    //month is 0 based
    return new Date(this.value, Year.schoolStartMonth + 1, 1);
  }

  get endOfYear() {
    //month is 0 based
    return new Date(this.value + 1, Year.schoolEndMonth + 1, 30);
  }

  convertToSchoolYear(date: MomentInput) {
    const mDate = moment(date).year(this.value);
    if ((mDate.month() + 1) >= Year.schoolStartMonth) return mDate;
    return mDate.add(1, 'y');
  }
}
