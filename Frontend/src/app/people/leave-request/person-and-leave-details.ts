import { Person } from '../person';
import { LeaveUsage } from '../self/self';

export class PersonAndLeaveDetails {
  person: Person;
  leaveUsages: LeaveUsage[];
}
