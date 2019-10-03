import { Pipe, PipeTransform } from '@angular/core';
import * as moment from 'moment';
import { DurationInputArg2 } from 'moment';

@Pipe({
  name: 'dateAdd',
  pure: true
})
export class DateAddPipe implements PipeTransform {

  //duration should be something like 1 day, or 18 years
  transform(value: any, duration: string): any {
    let values = duration.split(' ');
    return moment(value).add(parseFloat(values[0]), <DurationInputArg2> values[1]);
  }

}
