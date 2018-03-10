export class Role {
  public isDirectorPosition: boolean;
  public isStaffPosition: boolean;
  public fullHalfTime: string;

  constructor(public id?: string,
              public name?: string,
              public startDate?: Date,
              public active?: boolean,
              public endDate?: Date,
              public personId?: string) {
  }
}

export class RoleExtended extends Role {
  public preferredName: string;

  constructor(id: string,
              name: string,
              startDate: Date,
              active: boolean,
              endDate: Date,
              personId: string) {
    super(id, name, startDate, active, endDate, personId);
  }
}
