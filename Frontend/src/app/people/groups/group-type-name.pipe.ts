import {Pipe, PipeTransform} from '@angular/core';
import {GroupType, GroupTypeName} from './org-group';

@Pipe({
  name: 'groupTypeName'
})
export class GroupTypeNamePipe implements PipeTransform {

  transform(value: GroupType): any {
    return GroupTypeName(value);
  }

}
