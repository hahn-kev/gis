import { BaseEntity } from '../../classes/base-entity';
import { LeaveType } from '../self/self';

export class LeaveRequest extends BaseEntity {
  public personId: string;
  public startDate: Date;
  public endDate: Date;
  public type: LeaveType;
  public approved: boolean | null = null;
  public approvedById: string;
  public createdDate: Date;
  public reason: string;
  public days: number;
  public overrideDays: boolean;

  constructor() {
    super();
    this.createdDate = new Date();
    this.startDate = new Date();
    this.endDate = new Date();
  }
}


export class LeaveRequestWithNames extends LeaveRequest {
  public requesterName: string;
  public approvedByName: string;
}
