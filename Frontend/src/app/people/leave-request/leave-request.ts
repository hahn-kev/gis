export class LeaveRequest {

  constructor(public id?: string,
    public personId?: string,
    public startDate?: Date,
    public endDate?: Date,
    public type?: string,
    public approved?: boolean,
    public createdDate?: Date) {
  }
}

export class LeaveRequestWithNames extends LeaveRequest {
  public requesterName: string;
  public approvedByName: string;
}
