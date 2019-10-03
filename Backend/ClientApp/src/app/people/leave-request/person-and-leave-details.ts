import { PersonWithStaff } from '../person';
import { LeaveUsage } from '../self/self';

export class PersonAndLeaveDetails {
  person: PersonWithStaff;
  leaveUsages: LeaveUsage[];
}
