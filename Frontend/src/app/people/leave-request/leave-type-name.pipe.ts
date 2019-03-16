import { Pipe, PipeTransform } from '@angular/core';
import { LeaveType } from '../self/self';

@Pipe({
  name: 'leaveTypeName'
})
export class LeaveTypeNamePipe implements PipeTransform {

  transform(value: LeaveType): string {
    switch (value) {
      case LeaveType.SchoolRelated:
        return 'School Related';
      case LeaveType.MissionRelated:
        return 'Mission Related';
      default:
        return value;
    }
  }

}
