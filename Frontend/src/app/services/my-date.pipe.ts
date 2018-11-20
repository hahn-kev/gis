import { Optional, Pipe, PipeTransform } from '@angular/core';
import { DatePipe } from '@angular/common';

@Pipe({
  name: 'date'
})
export class MyDatePipe extends DatePipe implements PipeTransform {

  constructor(@Optional() locale: string = 'en') {
    super(locale);
  }

  transform(value: any, args?: any): any {
    if (!args) {
      args = 'MMM d, y';
    }
    return super.transform(value, args);
  }

}
