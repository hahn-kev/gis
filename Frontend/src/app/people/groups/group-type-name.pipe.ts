import { Pipe, PipeTransform } from '@angular/core';
import { GroupType } from './org-group';

@Pipe({
  name: 'groupTypeName'
})
export class GroupTypeNamePipe implements PipeTransform {

  transform(groupType: GroupType): string {
    switch (groupType) {
      case GroupType.Supervisor:
        return 'Office';
      default:
        return groupType;
    }
  }

}
