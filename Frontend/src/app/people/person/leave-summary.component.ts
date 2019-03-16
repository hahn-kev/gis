import { Component, Input } from '@angular/core';
import { Gender, } from '../person';
import { LeaveDetails } from '../self/self';

@Component({
  selector: 'app-leave-summary',
  templateUrl: './leave-summary.component.html',
  styles: []
})
export class LeaveSummaryComponent {
  @Input() personId: string;
  @Input() personGender: Gender;
  @Input() leaveDetails: LeaveDetails;
}
