import { Pipe, PipeTransform } from '@angular/core';
import { Degree } from './education';

@Pipe({
  name: 'degreeName'
})
export class DegreeNamePipe implements PipeTransform {

  transform(degree: Degree): string {
    if (degree == Degree.Associates) return 'Associates/HND';
    return degree;
  }

}
