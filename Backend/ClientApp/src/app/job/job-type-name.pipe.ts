import { Pipe, PipeTransform } from '@angular/core';
import { JobType } from './job';

@Pipe({
  name: 'jobTypeName'
})
export class JobTypeNamePipe implements PipeTransform {

  transform(type: JobType): string {
    switch (type) {
      case JobType.BlueCollar:
        return 'Blue Collar';
      default:
        return type;
    }
  }

}
